ADR 0001: Initial Architecture and Defaults

Status
- Accepted

Context
- MVP for a Controller + Display web app used on a church LAN, with Spanish-first translations.

Decision
- Frontend: Vue 3 + Vite + Pinia + Tailwind + Headless UI; PWA for offline support.
- Backend: .NET 8 Minimal API + SignalR; serve static web for same-origin.
- Storage: Start with SQLite + EF Core (LiteDB acceptable later for experimentation).
- Contracts-first: OpenAPI + realtime JSON Schemas in `shared/contracts` govern implementation.

Consequences
- Simpler LAN deployment (one process); minimal CORS concerns.
- Clear provider boundary; no licensed texts committed. Attribution supported.
- Future extensions (setlists, timers, stage view) will slot into the same contracts and boundaries.

