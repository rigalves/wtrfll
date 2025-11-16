# wtrfll

A LAN-friendly Controller + Display web app for projecting Bible passages.

## MVP Scope
- Create sessions with short codes and share controller/display links.
- Controller view: normalize references, fetch verses from local translations (RVR1960, NTV), publish state.
- Display view: join via token and render the published verses.
- Backend: .NET 8 Minimal API + SignalR + SQLite, normalized translation providers.
- Frontend: Vue 3 + Vite + Pinia + Tailwind.

## Getting Started
1. `dotnet run --project server/Wtrfll.Server.csproj`
2. `npm install && npm run dev --prefix web`
3. Open `http://localhost:5173/new` to create a session, then launch the Control/Display links provided.

---
See `docs/` for architecture, boundaries, and ADRs.
