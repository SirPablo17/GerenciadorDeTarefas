## 1. Domain

- [x] 1.1 Add `Number` (int) to `TaskItem` (`src/GerenciadorDeTarefas.Domain/Entities/TaskItem.cs`) and verify the project builds
- [x] 1.2 Add `NextTaskNumber` (int, default 1) to `User` (`src/GerenciadorDeTarefas.Domain/Entities/User.cs`) and verify the project builds
- [x] 1.3 Add `GetAndIncrementNextTaskNumberAsync(Guid userId, CancellationToken)` to `IUserRepository`, returning the newly assigned number
- [x] 1.4 Add an optional `TaskItemStatus? status = null` parameter to `ITaskRepository.ListByUserAsync`

## 2. Infrastructure

- [x] 2.1 Implement `GetAndIncrementNextTaskNumberAsync` in `UserRepository` using an atomic `UPDATE ... RETURNING` raw SQL statement (design.md - Decision 2), verified by the integration tests in section 5
- [x] 2.2 Update `TaskRepository.ListByUserAsync` to apply `.Where(t => t.Status == status.Value)` only when `status` is supplied, and update the EF Core entity configuration for the two new columns if explicit configuration is needed
- [x] 2.3 Add an EF Core migration for `TaskItem.Number` and `User.NextTaskNumber`, including a data migration that backfills existing rows per design.md - Decision 4 (per-user `CreatedAt` order, ties broken by `Id`; `NextTaskNumber` set to existing task count + 1), and verify `dotnet build` succeeds with the new migration present

## 3. Application

- [x] 3.1 Add `Number` (int) to `TaskDto`
- [x] 3.2 Update `TaskService.CreateAsync` to call `GetAndIncrementNextTaskNumberAsync` before constructing the `TaskItem`, assign the result to `Number`, and include it in the mapped `TaskDto`
- [x] 3.3 Add an optional `TaskItemStatus? status = null` parameter to `ITaskService.ListByUserAsync` / `TaskService.ListByUserAsync`, passed through to `ITaskRepository.ListByUserAsync`

## 4. Api

- [x] 4.1 Add an optional `TaskItemStatus? status` query parameter to `TasksController.List`, passed to `ITaskService.ListByUserAsync`, and manually verify via Swagger that `GET /tasks` (no filter), `GET /tasks?status=Completed` (valid filter), and `GET /tasks?status=NotAStatus` (invalid filter, expect 400) all behave as specced

## 5. Tests

- [x] 5.1 `TaskServiceTests`: creating a user's first task assigns `Number == 1` (mock `IUserRepository.GetAndIncrementNextTaskNumberAsync` to return 1, assert it flows into the returned `TaskDto`)
- [x] 5.2 `TaskServiceTests`: `ListByUserAsync` with no status passes `null` through to the repository and returns every task regardless of status, preserving current behavior
- [x] 5.3 `TaskServiceTests`: `ListByUserAsync` with a status passes it through to the repository and the returned list only contains tasks the (mocked) repository returned for that filter
- [x] 5.4 Integration test (`TasksEndpointsTests`): creating two tasks for the same user via `POST /tasks` yields sequential numbers (1, then 2) in the responses
- [x] 5.5 Integration test (`TasksEndpointsTests`): two different users each creating their first task both receive `Number == 1`
- [x] 5.6 Integration test (`TasksEndpointsTests`): deleting a user's task and then creating a new one yields a number greater than any previously assigned to that user (never reusing the deleted task's number)
- [x] 5.7 Integration test (`TasksEndpointsTests`): `GET /tasks?status=Completed` returns only that user's completed tasks; `GET /tasks` with no filter still returns all statuses; `GET /tasks?status=NotAStatus` returns `400`

## 6. Verification

- [x] 6.1 Run `dotnet build` and confirm the solution builds with no errors
- [x] 6.2 Run `dotnet test` and confirm all tests pass, including the new ones added in section 5
