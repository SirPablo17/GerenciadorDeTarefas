## Why

The app currently persists to a SQLite file (`gerenciadordetarefas.db`) on local disk. The planned deployment target, Render, uses an ephemeral filesystem — the `.db` file would be wiped on every redeploy, losing all data. Moving persistence to a hosted PostgreSQL database (Supabase) makes the deployment durable. This is purely an infrastructure/persistence-layer swap: no business logic, validation rule, or API-observable behavior is intended to change.

## What Changes

- Replace the `Microsoft.EntityFrameworkCore.Sqlite` package with `Npgsql.EntityFrameworkCore.PostgreSQL` in `GerenciadorDeTarefas.Infrastructure`.
- Switch `AddInfrastructure()` in `DependencyInjection.cs` from `options.UseSqlite(...)` to `options.UseNpgsql(...)`, still reading `ConnectionStrings:DefaultConnection`.
- Delete the SQLite-generated EF Core migrations and regenerate a fresh `InitialCreate` migration targeting PostgreSQL.
- Update connection strings: `appsettings.json` gets a placeholder (real value from an environment variable in production); `appsettings.Development.json` points at a Supabase Postgres project (dev) via the direct connection (port 5432, not the 6543 pgbouncer pooler); the local dev password is stored via `dotnet user-secrets`, never committed.
- Replace `CustomWebApplicationFactory`'s temp-file SQLite setup with an ephemeral PostgreSQL container via Testcontainers (`Testcontainers.PostgreSql`), so integration tests stay hermetic and don't depend on Supabase.
- Verify `UserRepository.GetAndIncrementNextTaskNumberAsync`'s raw SQL (`UPDATE ... RETURNING`) still works against PostgreSQL; PostgreSQL folds unquoted identifiers to lowercase while Npgsql's EF migrations typically quote mixed-case identifiers, so the raw SQL text may need to quote table/column names to keep matching the generated schema — this is a design/implementation detail to confirm during apply, not a behavior change.

None of this is expected to be **BREAKING** for API consumers — request/response shapes, validation, auth, and status codes are unchanged. It changes which database backend the app talks to and how integration tests provision their database.

## Capabilities

No spec-level (business-behavior) requirements change. This is a persistence-provider swap with identical externally observable API behavior, so this change sets `skip_specs: true` and introduces no new or modified capability spec.

### New Capabilities
_None._

### Modified Capabilities
_None._

## Impact

- **Code**: `src/GerenciadorDeTarefas.Infrastructure/GerenciadorDeTarefas.Infrastructure.csproj`, `DependencyInjection.cs`, `Persistence/AppDbContextFactory.cs` (design-time factory used by `dotnet ef`, currently hardcodes `UseSqlite`), `Persistence/Migrations/*` (regenerated), `Persistence/UserRepository.cs` (verify only, edit only if the raw SQL needs quoting).
- **Config**: `src/GerenciadorDeTarefas.Api/appsettings.json`, `appsettings.Development.json`, and local `dotnet user-secrets` for the dev DB password.
- **Tests**: `tests/GerenciadorDeTarefas.Tests/GerenciadorDeTarefas.Tests.csproj` (new `Testcontainers.PostgreSql` package), `tests/GerenciadorDeTarefas.Tests/Integration/CustomWebApplicationFactory.cs`.
- **Dependencies**: adds Npgsql EF Core provider and Testcontainers.PostgreSql; drops the SQLite EF Core provider. Docker becomes a local requirement for running the integration test suite.
- **External systems**: introduces a dependency on a Supabase-hosted PostgreSQL project for local development and (eventually) production; requires Docker locally for tests.
- **Out of scope**: `Application` services/validators, `Api` controllers, and the Angular frontend are untouched.
