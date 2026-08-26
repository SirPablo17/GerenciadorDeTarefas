## Why

The repository has no README, so there is no entry point explaining what this project is, how to run it, or why it exists. This project is a learning exercise for Spec Driven Development (SDD) and OpenSpec, and that context should be visible to anyone opening the repo.

## What Changes

- Add a `README.md` at the repository root that:
  - States that GerenciadorDeTarefas is a simple Task Management REST API built as a learning project.
  - Explains its purpose: learning the concepts of Spec Driven Development and the OpenSpec workflow (proposals, delta specs, design, tasks, archiving).
  - Gives a brief overview of the tech stack and project layout (.NET solution with Domain/Application/Infrastructure/Api layers).
  - Points to the `openspec/` directory (specs and changes) for anyone who wants to see the SDD/OpenSpec artifacts in practice.
  - Includes minimal instructions to build/run the API and run the tests.

## Capabilities

### New Capabilities
_None. This change adds project documentation only; it introduces no system behavior._

### Modified Capabilities
_None. No requirement-level behavior changes._

This is a documentation-only change (`skip_specs: true` in `.openspec.yaml`).

## Impact

- Adds `README.md` at the repository root. No source code, API behavior, or dependencies are affected.
