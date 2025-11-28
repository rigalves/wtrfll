Scope
- This file applies to the entire repository. All agents and contributors must follow these rules when creating or modifying files.

Principles
- Contracts first: REST and realtime schemas in `shared/contracts` are the source of truth. Do not implement endpoints or events that aren’t specified there.
- Boundaries: Keep concerns separated between `web/`, `server/`, and `shared/`. No cross-layer imports except from `shared` into `web`/`server`.
- Minimalism: Prefer simple, readable solutions over clever abstractions. Keep dependencies lean.
- Licensing: Only bundle public-domain Bible texts. Never commit licensed content. RVR1960/NTV require provider APIs and attribution.
- Externalize canonical metadata: Large data tables (Bible books, languages, etc.) must live in JSON/appsettings or dedicated data files, not as C# constants. Load them via providers so they can be extended without recompiling.
- Keep domain models in `Models/` folders: When a slice (e.g., translation tooling) needs reusable models, place each model class/record inside that slice’s `Models/` directory instead of embedding it inside providers or services.

Coding Conventions (must follow for all code)
- Descriptive names (long is good): Prefer clear, verbose names for variables, functions, methods, classes, and files. Include small words like `to`, `and`, `from`, `with`, `for` to improve readability. Avoid abbreviations and acronyms unless universally standard (`id`, `URL`, `HTTP`).
- No one-letter or cryptic names: Never use single-letter identifiers except for obvious counters in very small scopes. Choose names that describe intent and domain meaning.
- Small, single-purpose units: Keep functions/methods small and focused on one responsibility. Prefer extracting helpers over adding branching and parameters.
- Guard clauses and early returns: Avoid deep nesting. Check preconditions early and return fast when invalid.
- Parameter discipline: Avoid boolean flags that change behavior. Prefer separate functions or a typed options object. If more than 4 parameters, use a parameter object with named fields.
- Explicit types: Do not use implicit `any` in TypeScript. In C#, enable nullable reference types and use explicit types for clarity.
- Error handling: Fail fast with clear messages. Return structured errors over magic codes. Preserve provider attribution and cache policies in results.
- Avoid magic numbers/strings: Centralize in constants or enums. Document units (e.g., percentages for safe margins).
- Composition over inheritance: Prefer small composable modules and interfaces.
- Side effects: Prefer pure functions for parsing/formatting. Keep I/O at the edges (adapters, controllers, endpoints).
- Formatting and style: Follow repo formatters/configs if present; otherwise keep consistent casing (camelCase for JS/TS variables/functions, PascalCase for C# types and methods). File names in `web/` use kebab-case, C# files match type names.
- Localization text: Keep locale files in UTF-8 and preserve native characters (tildes, accents, punctuation). Do not replace language-specific glyphs with ASCII approximations or mojibake; if tooling mangles them, fix the source before committing.
- Tests: Name tests descriptively (behavioral sentences). Test the public surface; avoid over-mocking.

Repo Layout (enforced)
- `server/` — .NET 8 Minimal API + SignalR + storage + provider adapters. Depends on `shared/csharp` for DTOs.
- `web/` — Vue 3 + Vite + Pinia + Tailwind + Headless UI. Depends on `shared/typescript` for DTOs and schemas.
- `shared/` — Contracts and generated types only. No business logic.
- `docs/` — Architecture, boundaries, contracts overview, ADRs.

Allowed Dependencies
- `web` → `shared/typescript`
- `server` → `shared/csharp`
- `shared` → none

UI Conventions (web)
- Tailwind CSS + Headless UI (Vue). No component framework (no Quasar).
- Use Tailwind plugins: typography, forms, line-clamp.
- Components are small and focused. Keep state in Pinia stores under `src/stores/`.
 - Vue components should be concise: prefer Composition API with `script setup`. Keep components under ~200 lines; extract subcomponents when they grow.

Realtime Conventions
- SignalR events and payloads must match JSON Schemas in `shared/contracts/realtime/`.
- Emit minimal state; server validates and rebroadcasts authoritative `state.update`.

Versioning
- OpenAPI and realtime schemas carry a `contractVersion`. Any breaking change must bump it and regenerate types in `shared/typescript` and `shared/csharp`.

Assistant-Run Decision Flow
- The user may request architectural changes in plain English without writing ADRs.
- The assistant drafts the ADR and a Decision Brief for approval.
- After approval, the assistant updates ADRs, docs, and contracts, then implements code and tests in the same change (unless the user asks for a two-step flow).
- Non‑breaking, no‑contract “minor” changes (docs or internal refactors) can be auto‑approved if the user enables that preference.
 - Current policy: Auto‑proceed is ENABLED for minor, non‑breaking, no‑contract changes. Any contract or behavior changes still require explicit approval.

Do/Don’t
- Do add tests near changed logic (parser, providers, serialization).
- Do not introduce new endpoints/events without updating `shared/contracts` and docs.
- Do not widen public API surfaces without an ADR.

Size Guardrails (soft limits)
- Functions/methods: aim for ≤ 40 lines; extract helpers when exceeding.
- Classes/types: aim for ≤ 400 lines; split responsibilities when exceeding.
- Vue single-file components: aim for ≤ 200 lines per component section; move logic to composables in `src/lib/` when needed.
