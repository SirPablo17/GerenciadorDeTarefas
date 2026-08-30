## 1. Models

- [x] 1.1 Add `number: number` to `TaskDto` in `web/src/app/core/models.ts`, and verify `npm run build` (or `tsc`) succeeds

## 2. Task List Component — Tabs & Derived State

- [x] 2.1 Add a `selectedTab` signal (`'active' | 'completed'`, default `'active'`) to `TaskList` (`web/src/app/tasks/task-list/task-list.ts`)
- [x] 2.2 Add `activeTasks` and `completedTasks` `computed()` signals derived from `tasksService.tasks()` (Pending/InProgress vs. Completed), per design.md - Decision 1
- [x] 2.3 Add a `selectTab(tab: 'active' | 'completed')` method that updates `selectedTab`

## 3. Task List Template — Tabs, Empty States, Task Number

- [x] 3.1 Add tab navigation markup to `task-list.html` using DaisyUI's `.tabs` with `role="tablist"`/`role="tab"`/`aria-selected` and a task-count badge per tab, wired to `selectedTab()`/`selectTab()` (design.md - Decision 5)
- [x] 3.2 Update the loading/error/list rendering block to render from the currently selected tab's computed list (`activeTasks()`/`completedTasks()`) instead of `tasksService.tasks()` directly, keeping the existing loading-skeleton and error-with-retry states shared across both tabs
- [x] 3.3 Add the three distinct empty-state messages from design.md - Decision 4: no tasks at all (existing copy, shown on Active tab only), Active tab empty while completed tasks exist, Completed tab has no completed tasks (unconditional — same copy whether or not Active has tasks, including the zero-tasks-total case)
- [x] 3.4 Display each task's `number` (e.g., "#3") on the card, in both tabs

## 4. Tests

- [x] 4.1 New `web/src/app/tasks/task-list/task-list.spec.ts`: default tab is Active and shows only Pending/InProgress tasks, none Completed
- [x] 4.2 `task-list.spec.ts`: selecting the Completed tab shows only Completed tasks, none Pending/InProgress
- [x] 4.3 `task-list.spec.ts`: changing a task's status to Completed while on the Active tab removes it from the Active tab and it now appears under the Completed tab
- [x] 4.4 `task-list.spec.ts`: changing a Completed task's status back to Pending/InProgress while on the Completed tab removes it from the Completed tab and it now appears under the Active tab
- [x] 4.5 `task-list.spec.ts`: each of the three empty-state messages renders the expected distinct copy — no tasks at all, Active tab empty with completed tasks existing, and Completed tab empty (covering both: active tasks existing, and zero tasks total)
- [x] 4.6 `task-list.spec.ts`: a task's `number` is rendered on its card in both the Active and Completed tabs

## 5. Verification

- [x] 5.1 Run `npm test` in `web/` and confirm all tests pass, including the new ones added in section 4
- [x] 5.2 With the backend running (`dotnet run --project src/GerenciadorDeTarefas.Api --launch-profile http`) and `npm start` in `web/`, manually verify in the browser: task list opens on the Active tab, switching to the Completed tab works, marking a task Completed moves it to the Completed tab and reopening it moves it back, task numbers are visible on cards, and each of the three empty states displays correctly
