## 1. Package swap

- [x] 1.1 In `src/GerenciadorDeTarefas.Infrastructure/GerenciadorDeTarefas.Infrastructure.csproj`, remove the `Microsoft.EntityFrameworkCore.Sqlite` package reference and run `dotnet add src/GerenciadorDeTarefas.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL` (unpinned, to resolve the latest EF Core 10-compatible version); verify the `.csproj` no longer references the Sqlite package and now references Npgsql's provider.
- [x] 1.2 In `tests/GerenciadorDeTarefas.Tests/GerenciadorDeTarefas.Tests.csproj`, run `dotnet add tests/GerenciadorDeTarefas.Tests package Testcontainers.PostgreSql` (unpinned); verify the package appears in the `.csproj`.

## 2. Provider wiring

- [x] 2.1 In `src/GerenciadorDeTarefas.Infrastructure/DependencyInjection.cs`, replace `options.UseSqlite(configuration.GetConnectionString("DefaultConnection"))` with `options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))`; verify `dotnet build` succeeds.
- [x] 2.2 In `src/GerenciadorDeTarefas.Infrastructure/Persistence/AppDbContextFactory.cs`, replace `optionsBuilder.UseSqlite("Data Source=gerenciadordetarefas.db")` with `optionsBuilder.UseNpgsql(...)` using a syntactically valid placeholder connection string (e.g. `"Host=localhost;Database=gerenciadordetarefas;Username=postgres;Password=postgres"` — no real connectivity is needed here, see design.md's Decisions). This file implements `IDesignTimeDbContextFactory<AppDbContext>` and is what `dotnet ef` uses at design time, taking priority over `DependencyInjection.cs`; skipping this task would both break the build (once the SQLite package is removed in task 1.1, `UseSqlite` no longer resolves) and cause task 3.2 to generate a migration against the wrong provider. Verify `dotnet build` succeeds.

## 3. Migrations

- [x] 3.1 Delete `src/GerenciadorDeTarefas.Infrastructure/Persistence/Migrations/` (SQLite-specific, per design.md's Decisions); verify the directory no longer exists.
- [x] 3.2 Generate a fresh initial migration with `dotnet ef migrations add InitialCreate --project src/GerenciadorDeTarefas.Infrastructure --startup-project src/GerenciadorDeTarefas.Api --output-dir Persistence/Migrations` (depends on task 2.2 — `dotnet ef` builds the design-time model via `AppDbContextFactory`, and does not need real DB connectivity for a from-scratch migration); verify a new `Migrations/*_InitialCreate.cs` and updated `AppDbContextModelSnapshot.cs` are generated, both reflecting Postgres DDL (not SQLite), and `dotnet build` succeeds. Required two unplanned tooling fixes to unblock (see design.md's Decisions): pinning `Microsoft.EntityFrameworkCore`/`.Relational` to 10.0.11 in Infrastructure, and adding `Microsoft.EntityFrameworkCore.Design` to the Api project. `--output-dir Persistence/Migrations` was needed to match the existing project layout (EF's default is a top-level `Migrations/` folder).
- [x] 3.3 Update `UserRepository.GetAndIncrementNextTaskNumberAsync`'s raw SQL to quote identifiers (`"Users"`, `"NextTaskNumber"`, `"Id"`, `"Value"`) per design.md's Decisions, so it matches the case-sensitive quoted identifiers Npgsql's migration generates; verify by running the integration suite (task 6.2) — `TasksEndpointsTests` exercises task creation and therefore this method.

## 4. Configuration

- [x] 4.1 In `src/GerenciadorDeTarefas.Api/appsettings.json`, replace the `DefaultConnection` value (`Data Source=gerenciadordetarefas.db`) with a placeholder (e.g. `Host=;Database=;Username=;Password=`) plus a short comment/note that the real value comes from an environment variable in production; verify the file is still valid JSON (comments as a sibling `"//"`-style key or in `appsettings.json`'s own conventions — confirm the project's existing JSON files don't already contain comments before choosing a format) and the app doesn't accidentally connect using this placeholder.
- [x] 4.2 In `src/GerenciadorDeTarefas.Api/appsettings.Development.json`, add a `ConnectionStrings:DefaultConnection` pointing at the Supabase dev project's direct connection (host, database, username, port 5432 — not the 6543 pooler) with the password left out; verify by inspection that no password is present in this file.
- [x] 4.3 Run `dotnet user-secrets init --project src/GerenciadorDeTarefas.Api` (if not already initialized) and `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<full connection string with real password>" --project src/GerenciadorDeTarefas.Api` with the real Supabase dev password; verify `dotnet user-secrets list --project src/GerenciadorDeTarefas.Api` shows the key and that no secret was written to a tracked file (`git status` shows no changes beyond 4.1/4.2).

## 5. Integration test isolation

- [x] 5.1 Rewrite `tests/GerenciadorDeTarefas.Tests/Integration/CustomWebApplicationFactory.cs` to provision a `PostgreSqlContainer` (Testcontainers.PostgreSql) instead of a temp SQLite file: implement `IAsyncLifetime.InitializeAsync` to start the container before the host builds, inject its connection string via `ConfigureAppConfiguration`, and implement `DisposeAsync` to stop/remove the container; verify the class compiles and no longer references `_dbPath`/SQLite file cleanup.
- [x] 5.2 Verify `AuthEndpointsTests` and `TasksEndpointsTests` (which derive their fixture from `CustomWebApplicationFactory`) still run against the container-backed factory without other changes; verify by running `dotnet test --filter "FullyQualifiedName~AuthEndpointsTests|FullyQualifiedName~TasksEndpointsTests"`.

## 6. Verification

- [x] 6.1 Run `dotnet build` across the solution and verify it succeeds with no errors.
- [x] 6.2 Run `dotnet test` (Docker must be running locally for Testcontainers) and verify the full suite passes, including the raw-SQL task-numbering path exercised by `TasksEndpointsTests`.
- [x] 6.3 Run the API locally with `dotnet run --project src/GerenciadorDeTarefas.Api --launch-profile http` against the Supabase dev connection string (via user-secrets) and manually verify: register a user, log in, create/edit/complete/delete a task — confirming the corresponding rows appear in Supabase's Table Editor instead of a local `.db` file.
