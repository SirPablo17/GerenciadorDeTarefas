## Context

See proposal.md - Why. Relevant current state:
- `TaskItem` (Domain) has no human-readable identifier today, only `Id` (Guid).
- `User` (Domain) has no counter/sequence state.
- `TasksController.List` / `TaskService.ListByUserAsync` / `ITaskRepository.ListByUserAsync` return all of a user's tasks unconditionally, no filtering.
- Persistence is EF Core over SQLite, one file per environment; `Microsoft.Data.Sqlite` bundles a SQLite version with `UPDATE ... RETURNING` support (added in SQLite 3.35, well below what current `Microsoft.Data.Sqlite` ships).
- Single-process app (no multi-instance/horizontal scaling in this study project), but the API can still legitimately receive concurrent requests from the same user (e.g., a double-click or two browser tabs), so number generation must not race.

## Goals / Non-Goals

**Goals:**
- Generate a per-user sequential task number at creation time that is never reused, even across deletions.
- Let callers filter `GET /tasks` by status without changing the default (unfiltered) behavior.

**Non-Goals:**
- No frontend/UI work (explicitly out of scope per proposal.md).
- No renumbering, reordering, or letting clients set/edit a task's number.
- No multi-instance/distributed-safe counter design — SQLite's single-writer semantics are sufficient for this app's deployment shape.
- No general-purpose filtering/sorting framework for `GET /tasks` — only the single `status` filter requested.

## Decisions

### 1. Per-user counter lives on `User`, not derived from `MAX(Number)`
Add `NextTaskNumber` (int, default 1) to the `User` entity. Assigning a new task's number atomically increments this column and reads back the new value.

Rejected alternative: compute `MAX(Number) WHERE UserId = @id) + 1` at insert time. This breaks the "never reused" requirement — if a user's highest-numbered task is deleted, the next created task would reuse that number. A persistent counter avoids this by never decreasing regardless of deletions.

Rejected alternative: a separate `TaskNumberCounters` table keyed by `UserId`. Unnecessary indirection for a single int per user; a column on the existing `User` row is simpler and needs no extra join.

### 2. Atomic increment via raw `UPDATE ... RETURNING`
Implement the increment as a single parameterized statement executed through EF Core (`ExecuteSqlInterpolatedAsync`/`SqlQueryRaw`, whichever the codebase's existing raw-SQL convention favors):

```sql
UPDATE Users SET NextTaskNumber = NextTaskNumber + 1
WHERE Id = @userId
RETURNING NextTaskNumber - 1;
```

The returned value is the number assigned to the new task. This is a single atomic statement — SQLite's writer lock guarantees no two concurrent requests for the same user observe the same value, without needing an explicit `BEGIN TRANSACTION`/isolation-level dance in application code.

The counter increment does not need to share a transaction with the task `INSERT`. If the insert fails after the counter already advanced, the skipped number is simply never used — consistent with (and not a violation of) "never reused." This avoids coupling `IUserRepository` and `ITaskRepository` writes into a shared unit of work for this feature.

Exposed as a new `IUserRepository` method, e.g. `Task<int> GetAndIncrementNextTaskNumberAsync(Guid userId, CancellationToken)`, called from `TaskService.CreateAsync` before constructing the `TaskItem`.

### 3. Task number is server-generated only
`Number` is never accepted from the client. `CreateTaskRequest`/`UpdateTaskRequest` gain no `Number` field; `TaskDto` gains a read-only `Number` (int) field populated from `TaskItem.Number`.

### 4. Migration backfills existing tasks
For any tasks that already exist when this change is deployed, the EF Core migration backfills `Number` per user ordered by `CreatedAt` ascending (ties broken by `Id` for determinism: 1, 2, 3, ...), and sets each affected user's `NextTaskNumber` to `(that user's task count) + 1`. Users with no tasks yet default to `NextTaskNumber = 1`. This keeps numbering contiguous for current data instead of leaving pre-existing tasks with a sentinel/zero value.

### 5. Status filter: optional nullable enum query parameter
`GET /tasks` gains an optional `status` query parameter bound directly to `TaskItemStatus?` in the controller action. No filter present (or empty) preserves today's "return everything" behavior. An unparseable value (not one of the enum names) is rejected automatically by ASP.NET Core's model binding + `[ApiController]`'s automatic `400` response — no custom validation code needed for the "invalid status filter" scenario.

`ITaskService.ListByUserAsync` and `ITaskRepository.ListByUserAsync` gain a matching optional `TaskItemStatus? status = null` parameter; the repository applies `.Where(t => t.Status == status.Value)` only when a value is supplied.

## Risks / Trade-offs

- **[Risk]** Raw SQL (`RETURNING`) ties the increment to SQLite-specific syntax → **Mitigation**: this project is SQLite-only by design (see CLAUDE.md); if a future change swaps providers, this single repository method is the only place to adapt.
- **[Risk]** Backfill migration touches every existing task/user row → **Mitigation**: this is a study project with no production data; still, the migration is a standard EF Core migration and reviewable/revertible like any other.
- **[Risk]** Skipped numbers (from a counter increment whose task insert then fails) make numbers non-contiguous → **Mitigation**: acceptable — the spec requires numbers to be sequential-and-unique per user, not gap-free.

## Migration Plan

1. Add `NextTaskNumber` to `User` and `Number` to `TaskItem` in the Domain entities.
2. Add EF Core migration: new columns + data migration to backfill `Number`/`NextTaskNumber` for existing rows as described in Decision 4.
3. Apply migration via existing `dotnet ef database update` flow (or automatic migration-on-startup, whichever this project already uses — verify in `Program.cs`/`Infrastructure` before assuming).
4. No rollback complexity beyond the standard EF Core `Down()` migration (drop the two columns); no external systems depend on `Number` yet since the frontend is out of scope.
