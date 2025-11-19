# wtrfll

![wtrfll icon](web/public/icons/icon_128.png)

wtrfll is a LAN-friendly controller + display web app for projecting scripture, lyrics, and media in church gatherings.

## Features (MVP)

- Create/join sessions with short codes and auto-resuming links.
- Controller dashboard:
  - Normalize scripture references, fetch verses from offline translations (RVR1960, NTV), and broadcast them via SignalR.
  - Translation selector with offline badges and realtime fallback notices.
  - Display commands: show content, black screen, clear text, freeze display.
- Display view:
  - Token-based join, projector-first layout, subtle session badge, wake-lock to keep screens awake.
- Service worker + PWA manifest for quick reloads and future offline media.
- Backend: .NET 8 Minimal API + SignalR + SQLite with normalized translation providers.
- Frontend: Vue 3 + Vite + Pinia + Tailwind + Headless UI.

## Roadmap Highlights

- Controller remodel (ProPresenter-style slide list + tabbed editors).
- Lyrics storage (JSON sections) and media library.
- Image slides and styling presets.
- Tests for Pinia stores / realtime client.

## Getting Started

1. Backend: `dotnet run --project server/Wtrfll.Server.csproj`
2. Frontend: `npm install && npm run dev --prefix web`
3. Visit `http://localhost:5173/new` to create a session and open the controller/display links.

More docs: `docs/` (architecture, boundaries, UI flow, controller remodel plan).
