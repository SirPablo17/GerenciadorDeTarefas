## Context

See proposal.md - Why. Relevant current state:

- `Dockerfile` (repo root) already builds and publishes only `GerenciadorDeTarefas.Api` (`.NET SDK 10.0` → `aspnet:10.0` runtime), listens on port 8080, and has been verified against the real Supabase Postgres instance.
- `.dockerignore` currently excludes `web/` entirely from the Docker build context.
- The Api project (`Microsoft.NET.Sdk.Web`) has no `wwwroot/` today and no static-file middleware in `Program.cs`.
- `web/angular.json` sets no custom `outputPath` for the `web` project, so the Angular 22 application builder writes its production build to the default `web/dist/web/browser/`.
- `web/src/index.html` already has `<base href="/" />` and the app uses `provideRouter` (path-based routing, not hash routing) — both are what path-based static hosting with a fallback route needs; no frontend changes are required for this to work.

## Goals / Non-Goals

**Goals:**
- Single Docker image, single Render Web Service, serving both the API and the built Angular app on one origin.
- No CORS policy, no Angular `environment.ts`/base-URL introduced.
- Keep the existing dotnet-only layer caching behavior for the API build; add equivalent caching for the npm install step.

**Non-Goals:**
- Not optimizing Docker build time/image size beyond basic layer-caching (`package*.json` copied and `npm ci` run before the rest of `web/` is copied in, mirroring the existing `.csproj`-first pattern for the API).
- Not changing how `ng serve` / local development works — `web/proxy.conf.js` and the dev workflow in CLAUDE.md are unaffected.
- Not adding a health-check endpoint, custom caching headers, or a CDN — out of scope for this change.

## Decisions

**Add a Node build stage to the existing multi-stage Dockerfile, rather than a separate frontend build pipeline.**
A third stage (`node:22-alpine` or similar current LTS) runs `npm ci` + `npm run build` inside `web/`. Its output directory is copied into the final stage's `wwwroot/` alongside the published API. Alternative considered: build the Angular app outside Docker (e.g., in CI) and just `COPY` a pre-built `dist/` into the image. Rejected — it would require a CI step or manual build before every `docker build`/Render deploy, whereas Render only runs `docker build` on the repo as-is; keeping the Angular build inside the Dockerfile means `git push` is the only step needed.

**Serve static files from the default `wwwroot/` convention, not a custom path.**
ASP.NET Core's `UseStaticFiles()`/`UseDefaultFiles()` default to `<ContentRoot>/wwwroot`, and `ContentRoot` defaults to the published app's base directory. Copying the Angular build to `/app/wwwroot` in the final stage (same `WORKDIR /app` already used for the publish output) needs no extra configuration in `Program.cs` beyond calling the two middleware methods — no custom `StaticFileOptions.FileProvider`.

**Use `UseDefaultFiles()` + `UseStaticFiles()` + `MapFallbackToFile("index.html")`, not a custom middleware.**
- `UseDefaultFiles()` must run before `UseStaticFiles()` (it rewrites `/` to `/index.html` internally; `UseStaticFiles()` then serves it) — both placed early in the pipeline, before auth middleware (static assets need no auth).
- `MapFallbackToFile("index.html")` is registered as an endpoint alongside `MapControllers()`. Endpoint routing only falls through to it when nothing else (a controller route, a static file, or — in Development — the Swagger UI's own mapped endpoints) matched the request, which is exactly the "unknown GET path → let Angular Router try it" behavior the `frontend-hosting` spec calls for. This is the standard, documented ASP.NET Core pattern for hosting a SPA from the API itself, so no third-party package or custom fallback middleware is needed. Note: `MapFallbackToFile(string filePath)` without an explicit route pattern registers the fallback with the `{*path:nonfile}` constraint, which excludes any path whose last segment has a file extension — so a mistyped asset URL like `/img/typo.png` still 404s normally instead of returning `index.html`.

**Stop excluding `web/` in `.dockerignore`, but keep excluding its generated/local directories.**
Remove the blanket `web/` line; add `web/node_modules/`, `web/dist/`, and `web/.angular/` (Angular's local build cache) so the build context stays small and the image always builds the frontend fresh inside the container rather than trusting whatever happens to exist on the host.

## Risks / Trade-offs

- **[Risk]** Angular's default output path (`web/dist/web/browser`) is inferred from `angular.json`/project name, not hard-coded anywhere today → the Dockerfile's `COPY --from=web-build` line could point at the wrong path. **Mitigation**: tasks.md includes running `npm run build` locally first to confirm the exact output path before wiring the Dockerfile; a wrong path fails the `docker build` loudly (missing source path) rather than silently.
- **[Risk]** Image builds get slower (Node install + Angular production build added to every `docker build`) and frontend-only changes now require a full API+frontend image rebuild/redeploy. **Mitigation**: none needed for this project's scale (low-traffic study project, infrequent deploys); documented in proposal.md - Impact as an accepted trade-off. Splitting into two Render services later remains possible if this becomes a real problem.

## Migration Plan

1. Implement the Dockerfile, `Program.cs`, and `.dockerignore` changes.
2. Verify locally: `docker build -t gerenciador-local .` succeeds, then `docker run -p 8080:8080 -e ConnectionStrings__DefaultConnection=... -e Jwt__Key=... gerenciador-local` — confirm `GET /` serves the Angular app, a deep link like `/tasks` (typed directly) resolves via the fallback, and `/auth/login` + `/tasks` API calls still work end-to-end (register/login/create task).
3. Push to the branch Render deploys from. Render rebuilds the image from the root `Dockerfile` automatically (or manually trigger a deploy) — no change needed to Render's existing service configuration (still points at the repo root `Dockerfile`, still listens on the `EXPOSE`d port 8080, same required env vars as before: `ConnectionStrings__DefaultConnection`, `Jwt__Key`, etc.).
4. Rollback, if needed, is a plain revert of the commit (or redeploying the previous image) — no data migration or schema change is involved.
