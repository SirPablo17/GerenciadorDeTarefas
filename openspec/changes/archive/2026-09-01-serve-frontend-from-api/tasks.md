## 1. Confirm the Angular build output path

- [x] 1.1 Run `npm run build` in `web/` locally and record the exact output directory (expected `web/dist/web/browser` per `web/angular.json`'s default `outputPath`); use this confirmed path in the Dockerfile tasks below instead of assuming it.

## 2. Extend the Dockerfile with a Node build stage

- [x] 2.1 Add a `web-build` stage (current Node LTS image) that copies `web/package.json` + `web/package-lock.json` first, runs `npm ci`, then copies the rest of `web/` and runs `npm run build`; verify with `docker build --target web-build .` that it completes and produces the output directory confirmed in 1.1.
- [x] 2.2 In the final stage, `COPY --from=web-build` that output directory into `/app/wwwroot`; verify the full multi-stage `docker build -t gerenciador-local .` completes successfully.

## 3. Update `.dockerignore`

- [x] 3.1 Remove the blanket `web/` exclusion; add `web/node_modules/`, `web/dist/`, and `web/.angular/`; verify `docker build .` still succeeds after the change.

## 4. Serve the SPA from the API

- [x] 4.1 In `Program.cs`, add `app.UseDefaultFiles()` followed by `app.UseStaticFiles()`, positioned before the authentication/authorization middleware; verify by running the built image and confirming `GET /` returns `index.html` and a built static asset (e.g. the main JS bundle) returns 200.
- [x] 4.2 Add `app.MapFallbackToFile("index.html")` alongside `app.MapControllers()`; verify a client-side route typed directly (e.g. `GET /tasks/5/edit`) returns `index.html`, while an authenticated `GET /tasks` API call still returns JSON from `TasksController` — matches `specs/frontend-hosting/spec.md` scenarios "Deep link or refresh on a client-side route" and "API routes are not shadowed by the fallback".
- [x] 4.3 Using `--launch-profile http` in Development, verify `GET /swagger` still returns the Swagger UI and is not shadowed by the fallback — matches the "Swagger is not shadowed by the fallback in Development" scenario.
- [x] 4.4 Add a lightweight integration test in `tests/GerenciadorDeTarefas.Tests/Integration/` (using `CustomWebApplicationFactory`, following the pattern of `AuthEndpointsTests`/`TasksEndpointsTests`) confirming an authenticated `GET /tasks` still routes to `TasksController` and returns JSON rather than falling through to the static-file fallback, without depending on the Angular `wwwroot` build being present.

## 5. End-to-end verification

- [x] 5.1 Run the full image locally (`docker run -p 8080:8080 -e ConnectionStrings__DefaultConnection=... -e Jwt__Key=... gerenciador-local`) and manually walk through register → login → create/edit/complete/delete a task through the UI served at `http://localhost:8080`, confirming no CORS errors and all of the frontend's existing relative HTTP calls succeed unmodified.
- [x] 5.2 Run `dotnet build` and `dotnet test` and confirm both still pass — this change adds hosting/middleware wiring and Docker build steps only, no test-covered business logic changes.
