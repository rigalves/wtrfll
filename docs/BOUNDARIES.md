Purpose
Define module boundaries and allowed dependencies to keep the codebase maintainable.

Directories
- `web/` â€” Vue UI; depends only on `shared/typescript` for contracts.
- `server/` â€” .NET API/Hub; depends only on `shared/csharp` for contracts.
- `shared/` â€” Contracts and generated types; no dependencies.

Rules
- Contracts are the source of truth. Implementations must not drift from them.
- No business logic in `shared/`.
- No direct coupling between `web/` and `server/`.
- Providers implement a common interface; add a new provider via the adapter pattern.

Behavioral Boundaries (MVP)
- Pagination: verse-per-slide pre-pagination; do not split verses unless an ADR approves.
- Attribution: if required by provider contracts, the Display must render it and cannot hide it.

Ownership
- `web/` owns UI/UX and client state; `server/` owns authorization, validation, and authoritative session state.

Breaking Changes
- Require: ADR entry + contract version bump + types regeneration.

Style Boundaries (Code Generation)
- Naming: Prefer long, descriptive names for variables, functions, methods, classes, and files. Include connective words (`to`, `and`, `from`, `with`, `for`) to improve clarity. Avoid abbreviations unless standard (`id`, `URL`).
- Function size: Keep functions/methods small and single-purpose; extract helpers early. Avoid boolean parameters that toggle behavior; use typed options or separate functions.
- Structure: Favor composition and small modules over deep inheritance trees. Keep side effects at the edges (adapters/endpoints), and keep parsing/rendering logic pure.
- Types: No implicit `any` in TS; enable nullable reference types in C#. Prefer explicit, well-named types and enums over primitives.
- Errors: Use structured error results; avoid magic codes. Preserve provider `attribution` and `cachePolicy` as part of returned models.

Architecture Style (Enforced)
- Pattern: Modular Monolith organized as Vertical Slices (by feature), not by technical layers.
- Features (examples): `Sessions`, `Passages`, `Themes`, `Realtime`.
- Server layout per slice:
  - `Api/<Feature>/<Feature>Endpoints.cs` â€” Minimal API route mapping per feature.
  - `Application/<Feature>/...` â€” Requests (commands/queries), handlers, validators.
  - `Domain/<Feature>/...` â€” Entities/value objects/domain services.
  - `Infrastructure/...` â€” EF Core, providers, configuration (shared across slices via interfaces).
- Allowed dependencies (server):
  - Api â†’ Application
  - Application â†’ Domain (+ application-defined interfaces/ports)
  - Infrastructure â†’ Application interfaces/ports + Domain
  - Domain â†’ none
- Data access: Use EF Core DbContext directly in handlers. Do not add generic repositories unless an ADR approves it.
- Validation: One validator per request (e.g., FluentValidation). Keep mapping hand-written and explicit.
- Realtime: SignalR Hub contains zero business logic; it delegates to Application handlers and broadcasts contract-compliant events.
- Web (Vue) slices: group files by feature (components, store, api client). Use shared DTOs from `shared/typescript`; no ad-hoc models.
