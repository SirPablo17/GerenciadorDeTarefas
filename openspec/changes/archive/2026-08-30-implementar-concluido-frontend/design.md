## Context

See proposal.md - Why. Relevant current state:
- `TasksService` (`web/src/app/tasks/tasks.service.ts`) exposes `tasks`, `loading`, `error` signals and a single `load()` that does an unfiltered `GET /tasks`, populating `tasks` with every task the user owns.
- `TaskList` (`web/src/app/tasks/task-list/task-list.ts` + `.html`) renders one flat card grid from `tasksService.tasks()`, with its own loading-skeleton / error-with-retry / empty-state branches, and a per-card `<select>` that calls `changeStatus()` → `tasksService.update(...).subscribe(() => tasksService.load())`.
- The backend (`implementar-funcionalidade-concluido`, archived) now returns a `number` field on every `TaskDto` and accepts an optional `?status=` query filter on `GET /tasks`, but neither is consumed by the frontend yet.
- `TaskDto` in `web/src/app/core/models.ts` has no `number` field.

## Goals / Non-Goals

**Goals:**
- Split the task list into an Active tab (Pending + In Progress) and a Completed tab, with a task moving tabs immediately when its status crosses the Completed boundary.
- Display each task's `number` in both tabs.

**Non-Goals:**
- No backend changes (already done).
- No change to the create/edit task form.
- No pagination, sorting, or a third tab per individual status (per user decision: 2 tabs, not 3).
- No use of the backend's `?status=` filter for this change (see Decision 1) — it remains available for a future change (e.g. if the list needs server-side pagination).

## Decisions

### 1. Split tabs client-side from the single already-loaded list, not via two `?status=` requests
`TasksService.load()` keeps doing one unfiltered `GET /tasks` as today. `TaskList` derives two `computed()` signals from `tasksService.tasks()`:
- `activeTasks`: tasks with `status !== Completed`
- `completedTasks`: tasks with `status === Completed`

A `selectedTab` signal (`'active' | 'completed'`, default `'active'`) controls which computed list is rendered.

Rejected alternative: call `TasksService.load()` with a status filter per tab (using the backend's new `?status=` param), keeping a separate loading/error pair per tab. Rejected because it doubles network calls and loading/error state for no user-visible benefit at this app's scale — the whole task list is already fetched in one call today, and both tabs need it. Switching tabs becomes instant (no new request, no loading flash) with the computed-signal approach, which is a strictly better experience here.

Consequence: a single shared `loading`/`error` state (from `TasksService`) still covers both tabs, exactly as today. Only "empty" becomes tab-specific, since it's a property of the derived list, not of the fetch.

### 2. Tab-switch UI state lives in `TaskList`, not in `TasksService`
`selectedTab` is a plain component signal in `TaskList`, not part of `TasksService`. It's presentation state (which subset the user is currently looking at), not data the service owns; it does not need to survive navigation away from the screen, and keeping it local avoids leaking a UI concern into the data-fetching service.

### 3. Status-change-driven tab transfer needs no new logic
`changeStatus()` already reloads the full list via `tasksService.load()` after a successful update. Because `activeTasks`/`completedTasks` are `computed()` off `tasksService.tasks()`, a task whose status just crossed the Completed boundary automatically disappears from one computed list and appears in the other on the next change-detection cycle — no explicit "move" step needed.

### 4. Distinct empty-state copy per case
Three empty situations get distinct messages, extending today's single empty-state. The Completed tab's empty copy is intentionally unconditional (Active tab's population doesn't change it), which also covers the combinatorial case of a brand-new account with zero tasks at all where the user navigates straight to the Completed tab — that still falls under the third bullet below, with no separate "nothing at all yet" variant needed for that tab:
- No tasks at all yet (today's existing copy: "Você ainda não tem tarefas... Crie sua primeira tarefa.") — shown on the Active tab only, since that's the default tab and where task creation happens.
- Active tab empty but the user has completed tasks (e.g., "Nenhuma tarefa ativa. Todas as suas tarefas estão concluídas!"). Only applies when the user has at least one completed task — otherwise the "no tasks at all" copy above already covers the Active tab.
- Completed tab has no completed tasks (e.g., "Nenhuma tarefa concluída ainda."), regardless of whether the Active tab has tasks or is itself empty (including the zero-tasks-total case, if the user switches to this tab).

### 5. Tab markup follows DaisyUI's `tabs` component with proper ARIA roles
Use DaisyUI's `.tabs` class with `role="tablist"` on the container and `role="tab"` + `aria-selected` on each tab button, per CLAUDE.md's accessibility floor (semantic HTML before ARIA, nothing communicated by color alone — so the selected tab is also marked, not just styled differently). Each tab shows its task count badge (e.g., "Ativas (3)") so the distinction isn't color-only.

### 6. Task number display
Each card shows `#<number>` as a small muted label next to the title (e.g., `<span class="text-xs text-base-content/50">#{{ task.number }}</span>`), in both tabs. No separate sorting or grouping by number.

## Risks / Trade-offs

- **[Risk]** Client-side split (Decision 1) means the Completed tab always reflects everything the last `GET /tasks` returned, even as the list grows over time with no pagination → **Mitigation**: acceptable at this app's current scale (study project, no pagination anywhere yet); revisit if/when pagination is added.
- **[Risk]** No existing component-level tests exist for `TaskList` yet (only service-level tests, plus one component smoke test) → **Mitigation**: this change introduces `task-list.spec.ts` using Angular's `TestBed`/`ComponentFixture`, following the component-fixture setup already used in `app.spec.ts` (`TestBed.configureTestingModule` + `TestBed.createComponent`), combined with the injected-dependency mocking style used in `auth.guard.spec.ts`/`auth.interceptor.spec.ts` for `TasksService`/`AuthService`/`Router`.
