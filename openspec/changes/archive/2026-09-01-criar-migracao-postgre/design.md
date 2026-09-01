## Context

See `proposal.md` for motivation. Current state: `GerenciadorDeTarefas.Infrastructure` uses `Microsoft.EntityFrameworkCore.Sqlite` against a local `.db` file; `CustomWebApplicationFactory` (integration tests) creates one temp SQLite file per test run; `UserRepository.GetAndIncrementNextTaskNumberAsync` uses a raw interpolated SQL string with `UPDATE ... RETURNING` (`Persistence/UserRepository.cs:21-32`) to atomically claim the next per-user task number — it bypasses the EF Core query translator entirely. `TasksEndpointsTests` exercises task creation end-to-end and is today the only test that indirectly exercises this raw SQL path (no unit test mocks it, since it needs a real relational engine, not the in-memory provider). There's a second place the app constructs an `AppDbContext` outside DI: `Persistence/AppDbContextFactory.cs` implements `IDesignTimeDbContextFactory<AppDbContext>` and hardcodes `UseSqlite("Data Source=gerenciadordetarefas.db")` — this is the factory the `dotnet ef` CLI uses at design time, and it takes priority over `Program.cs`/DI when generating migrations.

## Goals / Non-Goals

**Goals:**
- Swap the EF Core provider from SQLite to PostgreSQL (via Npgsql) with zero change to `Application`/`Domain`/`Api` code or observable API behavior.
- Keep integration tests hermetic (no dependency on the real Supabase project) via an ephemeral Postgres container per test run.
- Preserve `GetAndIncrementNextTaskNumberAsync`'s atomic increment-and-return semantics.

**Non-Goals:**
- No change to entity shapes, DTOs, validators, or API contracts.
- No adoption of a snake_case naming convention or other broad EF Core Postgres idiom shift — this stays a minimal, same-shape provider swap.
- No production deployment/CI wiring for Render or Supabase secrets — that's a separate concern (deploy change), out of scope here. This change only prepares the codebase to run against Postgres.

## Decisions

**Npgsql.EntityFrameworkCore.PostgreSQL, unpinned version via `dotnet add package`.** It's the standard, actively-maintained EF Core provider for PostgreSQL and the only one with first-class EF Core 10 support. Not pinning lets `dotnet add package` resolve the latest version compatible with the installed EF Core 10 packages, consistent with how the other EF Core packages in this project are currently versioned (`10.0.11`).

**Regenerate migrations from scratch (delete `Persistence/Migrations/`, add fresh `InitialCreate`) rather than trying to port the SQLite migrations.** EF Core migrations embed provider-specific DDL (column types, `AUTOINCREMENT` vs `IDENTITY`/`serial`, etc.); SQLite migrations aren't valid against Npgsql's model differ. Since this app has no production data yet (SQLite `.db` is local/disposable), a clean `InitialCreate` is simpler and safer than a migration-history rewrite.

**Update `AppDbContextFactory.cs` to `UseNpgsql(...)` before generating the migration, using a syntactically valid placeholder connection string.** `dotnet ef migrations add` uses whatever `IDesignTimeDbContextFactory<AppDbContext>` is present in the assembly in preference to DI/`Program.cs`, so if this factory keeps calling `UseSqlite`, the `InitialCreate` migration would be generated against the wrong provider regardless of what `DependencyInjection.cs` says — and once the SQLite package is removed (proposal step 1), this file also stops compiling, since `UseSqlite` is an extension method from that package. Generating a from-scratch migration (no existing migration history to diff against) doesn't require the factory's connection string to actually be reachable — EF only needs a syntactically valid Npgsql connection string to build the design-time model, not a live connection — so this doesn't need to wait for real Supabase credentials from step 4.

**Quote identifiers in `GetAndIncrementNextTaskNumberAsync`'s raw SQL.** By default, EF Core (both SQLite and Npgsql providers) preserves the exact C# casing of table/column names (`Users`, `NextTaskNumber`, `Id`) and emits them as quoted identifiers in generated DDL — so the Postgres table is physically named `"Users"` (mixed-case, case-sensitive). PostgreSQL folds *unquoted* identifiers to lowercase before resolving them, so the existing unquoted raw SQL (`UPDATE Users SET NextTaskNumber = ...`) would resolve to the literal table name `users`, which won't exist — this fails at runtime (`relation "users" does not exist`), unlike SQLite which is not case-sensitive about unquoted identifiers here. The fix is to quote the identifiers in the raw SQL string to match EF's generated schema: `UPDATE "Users" SET "NextTaskNumber" = "NextTaskNumber" + 1 WHERE "Id" = {userId} RETURNING "NextTaskNumber" - 1 AS "Value"`. This is a one-line change scoped entirely to `UserRepository.cs`; it does not change the method's behavior or signature, only what's needed to keep the same behavior working on the new provider. `TasksEndpointsTests`, once running against the Testcontainers Postgres factory, exercises this path and would fail loudly if the quoting is wrong — no new dedicated test is needed beyond making sure that suite passes.

**Testcontainers.PostgreSql for integration tests, container lifecycle via `IAsyncLifetime`.** `WebApplicationFactory<Program>` doesn't have a natural async construction point, so `CustomWebApplicationFactory` must implement `IAsyncLifetime` (xUnit's async setup/teardown contract) to start the container in `InitializeAsync` before the host builds, and stop it in `DisposeAsync`. The container's connection string is injected via `ConfigureAppConfiguration` the same way the temp SQLite path is today. Testcontainers spins up a real, disposable Postgres per factory instance — no shared state between test runs, no dependency on the real Supabase project, consistent with CLAUDE.md's existing "one temp SQLite file per factory instance" isolation model, just swapping the backing engine. Requires Docker locally, which is already available in this environment.

**Direct connection (port 5432), not the Supabase pooler (6543/pgbouncer), for dev.** This app runs as a single process with EF Core's own ADO.NET connection pooling; pgbouncer's transaction-pooling mode adds constraints (e.g., no session-level features, prepared statement caveats) that solve a scaling problem this 3-user study app doesn't have. Direct connection is simpler and has no known incompatibilities with EF Core.

**Dev secret via `dotnet user-secrets`, not `appsettings.Development.json`.** Keeps the real Supabase dev password out of source control while still letting `appsettings.Development.json` document the non-secret parts of the connection (host, database, port) for anyone setting up the project locally.

**Explicitly pin `Microsoft.EntityFrameworkCore`/`Microsoft.EntityFrameworkCore.Relational` to 10.0.11 in Infrastructure, and add `Microsoft.EntityFrameworkCore.Design` (dev-only, `PrivateAssets="all"`) to the Api project.** Neither was anticipated in tasks.md but both were required to make the plan actually build/generate migrations:
- `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.3 depends on `Microsoft.EntityFrameworkCore`/`.Relational` `[10.0.4, 11.0.0)` — a floor of 10.0.4. Because `Microsoft.EntityFrameworkCore.Design`'s `PrivateAssets="all"` in Infrastructure stops its own (10.0.11) dependency requirement from flowing to the Api project, Api resolved EF Core to 10.0.4 while Infrastructure.dll was compiled against 10.0.11 — a hard `CS1705` assembly-version-downgrade error. Explicitly pinning both packages to 10.0.11 in Infrastructure (matching every other EF Core-adjacent package already pinned there) forces consistent resolution solution-wide.
- `dotnet ef migrations add --project Infrastructure --startup-project Api` needs `Microsoft.EntityFrameworkCore.Design`'s assembly loadable from the **startup** project's build output at runtime, not just the migrations project's. Since Infrastructure's `Design` reference is `PrivateAssets="all"` (intentionally, so it doesn't leak into consumers like `Api`'s own published output), Api's `bin/` never had `Microsoft.EntityFrameworkCore.Design.dll` — a `FileNotFoundException` inside dotnet-ef's tooling. This is Microsoft's documented multi-project EF Core setup gap; the fix is the standard one: reference `Design` in the startup project too, kept `PrivateAssets="all"` there as well since it's still dev/design-time tooling, not a runtime dependency of the shipped app.

## Risks / Trade-offs

- [Raw SQL / identifier casing mismatch breaks task numbering silently in production if missed] → Mitigated by the explicit quoting decision above, plus running the full integration suite (which exercises task creation, hence this path) against the new Postgres-backed test factory before merging.
- [Testcontainers requires Docker locally; CI or contributors without Docker can't run integration tests] → Already implicitly true today only for SQLite-free environments; Docker is confirmed available in this environment. Out of scope to add a CI Docker-less fallback in this change.
- [Fresh `InitialCreate` migration discards SQLite migration history] → Acceptable: no production data exists yet in the SQLite file (study project, pre-deploy), so there's nothing to preserve.
- [Supabase dev project connectivity/latency is an external dependency for local `dotnet run` testing] → Only affects manual/dev runs, not the automated test suite (which uses Testcontainers, not Supabase).

## Migration Plan

1. Swap package reference, `UseNpgsql(...)` in `DependencyInjection.cs`, and `UseNpgsql(...)` in `AppDbContextFactory.cs` (steps 1-2 of proposal).
2. Regenerate migrations against Postgres (design-time factory only needs a syntactically valid connection string, not real connectivity); fix the raw SQL identifier quoting (step 3).
3. Update connection strings and dev secrets (step 4).
4. Switch the integration test factory to Testcontainers (step 5).
5. Verify with `dotnet build` + `dotnet test`, then a manual smoke test against the real Supabase dev project (steps 6-7).

No rollback beyond reverting the branch/commit is needed: there's no production data or deployed environment depending on the SQLite path yet.
