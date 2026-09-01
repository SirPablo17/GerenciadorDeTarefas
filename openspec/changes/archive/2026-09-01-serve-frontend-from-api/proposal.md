## Why

The plan is to deploy to Render as a single Docker web service. The Angular frontend's `HttpClient` calls use relative paths (`/auth/login`, `/tasks`, etc.) with no `environment.ts`/base URL — this only works today because `ng serve`'s dev proxy forwards those paths to the API on `localhost:5246`. In production there is no such proxy. Serving the Angular production build from the same ASP.NET Core process that hosts the API keeps frontend and backend on one origin, so the existing relative paths keep working unmodified and no CORS policy is needed. The alternative (a separate static-site deploy) would require introducing a base-URL config to every Angular HTTP call and enabling CORS on the API — more moving parts for a study project with one deploy target.

## What Changes

- `Dockerfile`: add a Node build stage that runs `npm ci` + `npm run build` inside `web/`, producing the Angular production build (`web/dist/web/browser`, per `web/angular.json`'s default output path for the `web` project). Copy that output into the final .NET image's `wwwroot/` alongside the published API.
- `src/GerenciadorDeTarefas.Api/Program.cs`: add static file serving (`UseDefaultFiles()` + `UseStaticFiles()`) and a fallback route to `index.html` for GET requests that don't match a controller route or an existing static file, so Angular Router's client-side routes (e.g. `/tasks/5/edit` typed directly in the browser) resolve correctly. The fallback must not intercept `/auth/*`, `/tasks/*`, or (in Development) `/swagger/*`.
- `.dockerignore`: stop excluding `web/` from the Docker build context (still exclude `web/node_modules/`, `web/dist/`, and other generated/local-only paths) so the new Node build stage can see the Angular source.
- No changes to CORS (not needed — frontend and API become same-origin), and no `environment.ts`/base-URL config added to the Angular app — its existing relative HTTP calls are left as-is.

## Capabilities

### New Capabilities
- `frontend-hosting`: the API process serves the built Angular SPA (static assets + client-side-routing fallback to `index.html`) alongside its existing REST endpoints.

### Modified Capabilities
_None._ `user-auth`, `task-management`, and `task-management-ui`'s existing requirements (endpoint contracts, validation, UI behavior) are unchanged — this only changes how the already-built frontend is delivered to the browser.

## Impact

- **Code**: `Dockerfile`, `.dockerignore`, `src/GerenciadorDeTarefas.Api/Program.cs`.
- **Build**: the Docker image build now requires Node.js (a new build-stage dependency) in addition to the .NET SDK; image build time increases (`npm ci` + Angular production build on every image build).
- **Config**: none — no new environment variables. Existing `ConnectionStrings__DefaultConnection` and `Jwt__*` env vars (already required for the API alone) are unchanged.
- **Deployment**: enables deploying the whole app (API + frontend) as a single Render Web Service from the repo's root `Dockerfile`, no separate static-site service or CORS configuration needed.
- **Out of scope**: Angular application code (components, services, routing), the API's business endpoints, authentication, and the Postgres/Supabase persistence layer are untouched.
