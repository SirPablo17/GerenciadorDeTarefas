# frontend-hosting Specification

## Purpose

Lets the API process deliver the built Angular frontend directly to browsers, so the whole app (API + UI) runs as a single deployable service with one origin and no separate frontend host.

## Requirements

### Requirement: Static Frontend Asset Serving
The system SHALL serve the Angular production build's static files (`index.html` and its built JS/CSS/asset files) from the API process, including at the root path `/`.

#### Scenario: Requesting the app root
- **WHEN** a client sends `GET /`
- **THEN** the system responds with the frontend's `index.html`

#### Scenario: Requesting a built static asset
- **WHEN** a client sends `GET` for a file present in the Angular production build's output (e.g. a hashed JS or CSS bundle)
- **THEN** the system responds with that file's contents and a content type appropriate to the file

### Requirement: Client-Side Routing Fallback
The system SHALL respond to a `GET` request with `index.html` when the request does not match an API route, an existing static asset, or (in Development) a Swagger route, so Angular Router can resolve client-side routes on a full page load or browser refresh.

#### Scenario: Deep link or refresh on a client-side route
- **WHEN** a client sends `GET /tasks/5/edit` (a route owned by Angular Router, not by any API controller)
- **THEN** the system responds with `index.html` so the Angular app loads and resolves the route client-side

#### Scenario: API routes are not shadowed by the fallback
- **WHEN** a client sends a request matching an existing API route (e.g. `GET /tasks`, `POST /auth/login`)
- **THEN** the system routes the request to the corresponding controller action, not to the static-file fallback

#### Scenario: Swagger is not shadowed by the fallback in Development
- **WHEN** the application is running in the Development environment and a client sends `GET /swagger`
- **THEN** the system responds with the Swagger UI, not with `index.html`
