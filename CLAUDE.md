# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Task management app (REST API in .NET + Angular web client) built as a **study project** to practice **Spec Driven Development (SDD)** via **[OpenSpec](https://github.com/Fission-AI/OpenSpec)**. It is not intended for production use — README.md (in Portuguese) has full narrative context on goals and stack.

## OpenSpec workflow

Every change to the system goes through `openspec/` before/while it's implemented:

- `openspec/specs/` — main specs describing current system behavior (`user-auth`, `task-management`, `task-management-ui`).
- `openspec/changes/` — in-progress changes, each with `proposal.md`, delta specs, optional `design.md`, and `tasks.md`.
- `openspec/changes/archive/` — completed changes, kept for history; a good reference for how a feature was proposed/designed before implementation.

When asked to add or modify a feature, follow this flow rather than editing code directly: propose the change (`openspec-propose`), implement its tasks (`openspec-apply-change`), sync deltas into `openspec/specs/` (`openspec-sync-specs`), then archive (`openspec-archive-change`). All changes in `openspec/changes/` are currently archived (i.e. there is no change in flight) — check there first to see if one is already in progress before starting new work.

## Commands

### Backend (`src/`, .NET 10)

```bash
dotnet build                                                          # build the whole solution
dotnet run --project src/GerenciadorDeTarefas.Api --launch-profile http   # run the API (Swagger at /swagger)
dotnet test                                                            # run all tests
dotnet test --filter "FullyQualifiedName~TaskServiceTests"             # run a single test class
dotnet test --filter "FullyQualifiedName~TaskServiceTests.MethodName"  # run a single test
```

**Always use `--launch-profile http`, never `https`.** `launchSettings.json` defines both; the `https` profile also opens port 7034, and `UseHttpsRedirection` then redirects every HTTP call there. The frontend's dev proxy only forwards that redirect instead of following it, and the dev HTTPS cert isn't trusted (`dotnet dev-certs https --trust` isn't set up), so with `https` selected, login (and everything else) breaks in the frontend.

### Frontend (`web/`, Angular 22)

```bash
cd web
npm install
npm start                     # ng serve at http://localhost:4200, proxies /auth and /tasks to the API
npm test                      # ng test (Vitest via @angular/build:unit-test)
npx ng test --watch=false path/to/some.spec.ts   # run a single spec file
npm run build                 # ng build
```

The dev server proxy (`web/proxy.conf.js`) forwards `/auth` and `/tasks` to `http://localhost:5246`. The API must be running (with the `http` profile) before login/register/tasks screens will work.

## Architecture

Backend is layered (Clean Architecture-ish), frontend is a separate Angular project outside the .NET solution:

```
src/GerenciadorDeTarefas.Domain/          Entities (TaskItem, User) and repository interfaces — no dependencies on other layers
src/GerenciadorDeTarefas.Application/     Services (AuthService, TaskService), DTOs, FluentValidation validators, app-level exceptions
src/GerenciadorDeTarefas.Infrastructure/  EF Core (SQLite) persistence, repository implementations, JWT token generation, password hashing
src/GerenciadorDeTarefas.Api/             Controllers, ExceptionHandlingMiddleware, Program.cs composition root
tests/GerenciadorDeTarefas.Tests/         Application-level unit tests (services, validators) + Integration tests (via CustomWebApplicationFactory, one temp SQLite file per factory instance)
web/src/app/core/                         Cross-cutting frontend concerns: AuthService (session/localStorage), auth.guard (route protection), auth.interceptor (attaches JWT, handles 401 -> logout+redirect)
web/src/app/login/, register/, tasks/     Feature areas (screens + services), one folder per route
```

Dependency direction: `Api -> Application -> Domain`, with `Infrastructure` implementing `Application`/`Domain` abstractions and wired up via `Infrastructure.DependencyInjection.AddInfrastructure()`, called from `Program.cs`. Controllers depend on `Application` service interfaces (`IAuthService`, `ITaskService`), never on `Infrastructure` directly.

Auth: JWT bearer tokens (`GerenciadorDeTarefas.Infrastructure.Auth`), configured from the `Jwt` section in `appsettings.json`. Every `/tasks` endpoint requires a valid token and is scoped to the authenticated user. On the frontend, the token lives in `localStorage`; any `401` from an authenticated call ends the session and redirects to `/login`, preserving the originally requested route so the user returns to it after logging back in (see archived changes' `design.md` for the `localStorage`-vs-`httpOnly-cookie` trade-off discussion).

Frontend uses standalone components + signals (no NgRx), Angular Material for UI, and Reactive Forms with validation mirroring the API's rules.

## Other directories

- `prototipo/` — Claude Design canvas prototype/mockups (`.dc.html` artboards + exported standalone HTML), not part of the running app.
