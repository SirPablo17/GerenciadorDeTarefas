## Context

The Angular app (`web/`) currently styles every screen with Angular Material (`mat-card`, `mat-form-field`, `mat-select`, `mat-toolbar`, `MatDialog` for the delete confirmation, `mat-icon` font ligatures) on the CLI default theme — no design was ever agreed for it. `prototipo/` now holds an approved high-fidelity prototype for the same 4 screens (Login, Cadastro, Tarefas, FormularioTarefa), built by a design tool against a custom "nocturne" token set (`prototipo/_ds/nocturne-*/styles.css`): dark background `#161826`, surface `#232532`, text `#e9e9ed`, purple accent `#9184d9`, plus neutral/accent 100–900 ramps and `radius-sm/md/lg` (4/8/14px). See proposal.md - Why for why we're porting this now instead of iterating on Material's theme.

Constraints:
- Angular 22, built with `@angular/build` (esbuild-based); no other JS framework (per the original prototype brief in `prototipo/uploads/especificacao-prototipo-task-manager.md`).
- No backend changes; API contracts and the `task-management-ui` spec's observable behavior stay exactly as-is (see proposal.md - Non-goals).
- Reactive Forms stay as the form mechanism; only the presentation layer changes.

## Goals / Non-Goals

**Goals:**
- Add Tailwind CSS + DaisyUI to `web/` and fully remove `@angular/material` + `@angular/cdk`.
- Define one custom DaisyUI theme that reproduces the prototype's dark/purple palette, so the four screens visually match `prototipo/export-src/*.html`.
- Replace every Material-specific primitive (form field, select, dialog, icon font, spinner) with a Tailwind/DaisyUI or native-HTML equivalent, with no residual Material dependency (styling or JS).

**Non-Goals:**
- Pixel-for-pixel reproduction of the prototype's exact spacing scale (`--space-1: 2.8px`, etc.) — that scale is an artifact of the design tool, not a deliberate design decision; Tailwind's default spacing scale is close enough and far easier to maintain.
- A light theme or theme switcher — one dark theme (matching the prototype) is all that's needed.
- Forgot-password, social login, or a grid/list layout toggle — these appear in the prototype only as decorative or design-tool-only affordances with no backing requirement (see proposal.md - Non-goals).

## Decisions

**1. Tailwind CSS v4 + DaisyUI v5, CSS-first config (no `tailwind.config.js`).**
Add `tailwindcss`, `@tailwindcss/postcss`, `daisyui` as dev dependencies, register `@tailwindcss/postcss` via a `.postcssrc.json` at `web/`'s root (Angular's esbuild-based builder already runs configured PostCSS plugins over global and component styles — no custom builder needed), and do everything else in `web/src/styles.css`: `@import "tailwindcss";` + `@plugin "daisyui";`. This avoids a `tailwind.config.js`/content-glob setup and matches how current Tailwind/DaisyUI major versions are meant to be used.
*Alternative considered*: Tailwind v3 + `tailwind.config.js` (content globs over `web/src/**/*.html,ts`) + `daisyui` in the `plugins` array. Rejected — more config surface for no benefit on a project this size.

**2. One custom DaisyUI theme sampled from the prototype's tokens, not a stock DaisyUI theme.**
Define a `gerenciador-dark` theme via `@plugin "daisyui/theme"` in `styles.css`, mapping DaisyUI's semantic color slots to the prototype's actual values: `base-100` ← `#161826`, `base-200` ← `#232532`, `base-content` ← `#e9e9ed`, `primary` ← `#9184d9`, plus the neutral/accent ramps for hover/muted states, and `--radius-box`/`--radius-field` from the prototype's `radius-md`/`radius-sm`.
*Alternative considered*: a built-in dark DaisyUI theme (`dracula`, `night`, `synthwave`, …). Rejected — none reproduce the approved prototype's specific palette, and the whole point of this change is to match it.

**3. Delete-confirmation modal: native `<dialog>` + DaisyUI `.modal` classes, no CDK.**
Wrap the native `<dialog>` element (`showModal()`/`close()`) in a small standalone component, styled with DaisyUI's `.modal`/`.modal-box` classes — the same pattern the prototype uses conceptually. Keeps the built-in focus trap, `Esc`-to-close, and backdrop click behavior for free.
*Alternative considered*: keep `@angular/cdk` (drop only `@angular/material`) and use `@angular/cdk/dialog` for the overlay. Rejected — the proposal scopes out both packages, and native `<dialog>` removes the last reason to keep CDK around.

**4. Icons: inline SVG ported from the prototype, not an icon font or library.**
Copy the SVG markup already used in `prototipo/export-src/*.html` directly into the Angular templates. No new icon dependency, and it's pixel-identical to what was approved.
*Alternative considered*: an icon library (Lucide, Iconify, Material Symbols font). Rejected — unnecessary new dependency; the prototype's own SVGs already cover every icon this app uses (add, edit, delete, logout, error, empty state).

**5. Task status control: segmented button group in the form, inline `<select>` in the list — matches the prototype exactly.**
Form: DaisyUI `join` of radio-styled buttons (prototype's `.seg`/`.seg-opt`) replaces `mat-select`. List: stays a compact `<select class="select select-sm">` per task row (DaisyUI), same pattern the prototype and the current app both already use — no reason to change it.

**6. Task list layout: single responsive grid of cards, matching the prototype's default.**
`grid-cols-[repeat(auto-fill,minmax(280px,1fr))]` (Tailwind arbitrary value) replaces today's vertical list of full-width `mat-card`s. No grid/list toggle is exposed to the user — the prototype's toggle was a design-tool preview prop, not a product feature (see proposal.md - Non-goals).

## Risks / Trade-offs

- [Risk] Angular's esbuild PostCSS pipeline may need verification against Tailwind v4's newer `@import`/`@plugin` CSS syntax before it's proven working in this exact CLI version → Mitigation: verify with `npm start` right after the dependency/theme setup step, before porting any screen, so pipeline issues surface early.
- [Risk] Native `<dialog>` doesn't automatically replicate every accessibility behavior `MatDialog` provided out of the box (e.g., return-focus-on-close nuances) → Mitigation: manually test keyboard (`Tab`/`Esc`) and screen-reader behavior on the delete-confirmation dialog before calling the port done; keep the `role="dialog"`/`aria-modal`/`aria-labelledby` attributes the prototype already sets.
- [Risk] Removing Material's form-field wiring (floating labels, `aria-describedby` error announcements) loses that built-in a11y plumbing → Mitigation: wire `<label for>` and `aria-describedby` by hand in the new templates, same as the prototype's markup already does.
- [Risk] Deleting `@angular/material`/`@angular/cdk` can break existing spec files that import Material test harnesses or modules → Mitigation: update/remove those imports as each component is ported, and run `npm test` after every screen to catch breakage immediately.

## Migration Plan

No persisted data or user-facing state is involved, so this ships as a single in-place swap rather than a staged rollout:
1. Add Tailwind + DaisyUI + the `gerenciador-dark` theme; verify the dev server renders the new base styles.
2. Port Login and Cadastro (Register) — simplest screens, establishes the field/button/card patterns reused everywhere else.
3. Port Tarefas (Task List): grid layout, loading skeleton, error/empty states, inline status `<select>`, native-`<dialog>` delete confirmation.
4. Port FormularioTarefa (Task Form): segmented status control, validation styling.
5. Remove `@angular/material`, `@angular/cdk`, and `web/src/material-theme.scss`; confirm no `mat-*` usage remains.
6. Update `CLAUDE.md`'s frontend stack description (Angular Material → Tailwind + DaisyUI).

Rollback: revert the change's commit(s) — there's no data migration or backend coupling to unwind.
