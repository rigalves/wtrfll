Source of Truth
- REST: `shared/contracts/openapi.yaml` (OpenAPI 3.x)
- Realtime: `shared/contracts/realtime/*.schema.json` (JSON Schema 2020-12)

Versioning
- All contracts include `contractVersion`. Breaking changes require bumping this value and regenerating types.

Endpoints (defined in OpenAPI)
- POST `/api/sessions` — create session (id, shortcode, join tokens)
- POST `/api/sessions/{id}/join` — join as controller/display using join token; controller lock enforced
- GET `/api/bibles` — list translations (provider, cache policy)
- GET `/api/passage` — normalized verses and attribution from local/proxy

Realtime Events (schemas)
- `state.patch` — controller→server: desired state changes
- `state.update` — server→group: authoritative state
- `control.lock` — server→group: controller lock status
- `heartbeat` — ping/pong with timestamps

Type Generation (planned)
- TS types: generate from OpenAPI and JSON Schemas into `shared/typescript/`
- C# types: generate into `shared/csharp/`

Attribution Rules
- Provider responses include `attribution` metadata and `cachePolicy` (OpenAPI: `PassageResult.attribution/cachePolicy`).
- If `attribution.required = true`, the Display must show attribution text (e.g., small corner text) and must not allow hiding it.
- If `required = false`, the Display may allow toggling attribution visibility via settings.
- Web and server implementations must not persist licensed text beyond the provider's allowed cache policy.
