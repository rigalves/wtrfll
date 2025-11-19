# wtrfll MVP Roadmap (Updated)

wtrfll is a controller + display scripture tool built for a single church LAN. The stack remains Vue 3 + Pinia + Tailwind on the frontend and .NET 8 Minimal APIs + SignalR on the backend. This document captures the current MVP state (late 2025) and the last few work items left before we call it “done”.

## Goals

1. **Simple operator experience** – publish references quickly, preview them, and push to the projector tab in real time.
2. **Shared state** – controller and display stay synchronized through SignalR groups and offline-ready translations.
3. **Open-source friendly** – clear architecture docs, translation licensing guardrails, and tooling for importing text.
4. **Extendable slices** – server code follows vertical slices (Sessions, Passages, Bibles, BibleBooks) so future pastoral tooling plugs in cleanly.

## Current Build

### Frontend
- Vue 3 + Vite + Pinia + Tailwind (typography/forms plugins) with Vue Router + vue-i18n.
- Router views:
  - `/` landing role selector with Controller/Display cards powered by `/api/sessions/upcoming`.
  - `/new` form to name/schedule a session, create it, and show controller/display links with copy buttons.
  - `/control/:sessionId?token=` controller dashboard (reference input, normalized preview, translation selector, verse stepping, display commands).
  - `/display/:sessionId?token=` display surface tuned for Chromecast + wake lock.
- Pinia stores:
  - `bibleBooksStore`: loads canonical metadata with a global spinner overlay and auto retry while the backend boots.
  - `upcomingSessionsStore`: caches upcoming sessions (name, short code, tokens, schedule) for the landing view.
  - `biblesStore`: fetches `/api/bibles` and flags offline-ready translations.
  - `referenceInputStore`: manages draft inputs and recent history.
  - `sessionParticipationStore`: handles REST joins, caches tokens per role, and now stores the friendly session name/schedule.
  - `sessionStore`: parses references, calls `/api/passage`, and pushes/receives SignalR patches/updates (verses, commands, presentation options).
- Reusable utilities: OpenAPI client via `openapi-fetch`, `SessionRealtimeClient` wrapper, reference parser helpers, wake-lock service.

### Backend
- .NET 8 Minimal API hosting:
  - `/api/sessions` (create named/scheduled sessions, join existing ones, list upcoming sessions).
  - `/api/passage`, `/api/bibles`, `/api/bible-books`.
  - SignalR hub `/realtime` enforcing join tokens.
- SQLite via EF Core (sessions + participants) with a migration adding session names and schedules.
- Translation tooling (`tools/Translations`) converts legacy JSON or OSIS/Zefania inputs to normalized JSON saved under `server/Data`.
- Providers (RVR1960 + NTV via normalized JSON) plus bible book metadata served through dedicated slices.
- Vertical slices:
  - `Slices/Sessions`: lifecycle service, query service, REST endpoints, SignalR hub + connection registry.
  - `Slices/Passages`: translation providers + scripture parser helpers.
  - `Slices/Bibles`: translation catalog.
  - `Slices/BibleBooks`: metadata loader wired to the bible-book API.

## MVP Checklist

| Area | Status | Notes |
|------|--------|-------|
| Session creation & join | [x] | REST endpoints accept optional name/schedule; Pinia flow stores metadata. |
| Landing role selector | [x] | `/` shows Controller/Display cards, sorted upcoming sessions, and deep-links with tokens. |
| Local translations (RVR1960, NTV) | [x] | Normalized JSON + provider wiring + fallback notice. |
| Real-time sync (SignalR) | [x] | Controller publishes patches; display listens to `state.update`. |
| Reference parser | [x] | Supports Spanish/English aliases, ordinals, ranges. |
| Bible book metadata loading | [x] | Spinner overlay with auto retry until backend ready. |
| Translation catalog UI | [x] | Controller sidebar lists `/api/bibles` with offline badges. |
| Chromecast-ready display view | [x] | Minimal UI with reference, verses, attribution, wake lock. |
| Offline translation handling | [x] | Local normalized translations auto-badge as offline ready; controller falls back when online sources fail. |
| Clear/black/freeze commands | [x] | Controller buttons drive `displayCommand` in realtime patches. |
| PWA + wake lock | [x] | Manifest + service worker + wake-lock helper inside DisplayView. |
| Basic tests (Vitest) | [ ] | Still pending for session store + parser. |

## Deferred / Next Wave

- Setlists and sermon flow builder.
- Stage view, timers, remote phone control.
- Advanced themes/backgrounds or animations.
- Cloud syncing / multi-tenant features.

## Immediate Next Steps

1. **Surface session metadata in control/display views** – show the friendly name + scheduled time from `sessionParticipationStore` so operators always know which service they are controlling.
2. **Basic Vitest coverage** – at minimum exercise `sessionStore` realtime handling and the bible-book/upcoming-session loaders.
3. **Share-friendly join helpers** – QR codes or OS share sheet buttons for controller/display links to remove any remaining copy/paste friction (especially for external projector laptops).

## Technology Summary

| Layer | Stack |
|-------|-------|
| Frontend | Vue 3, Vite, TypeScript, Pinia, Vue Router, Tailwind CSS, Headless UI, vue-i18n |
| Backend | .NET 8 Minimal APIs, EF Core (SQLite), SignalR |
| Tooling | Node 18+ for Vite, .NET 8 SDK for server/tooling, OpenAPI + `openapi-typescript` for client types |
| Deployment | Single self-hosted server on the LAN serving both static files and APIs; same-origin to avoid CORS |

## Licensing Guardrails

- Ship only translations with public-domain status or explicit permission. RVR1960 and NTV are normalized derivatives stored under `server/Data`.
- `tools/Translations` should run offline; add licensing notes in `docs/Licensing.md` when new sources are introduced.
- Display view renders attribution text returned by the passage provider.

## LAN Deployment Notes

1. Run `dotnet run --project server/Wtrfll.Server.csproj` to start the API + SignalR hub + static files.
2. Run `npm install` once, then `npm run dev --prefix web` (or `npm run build && npm run preview`) for the frontend during development.
3. Start the backend before opening the browser; the app shows “Cargando catálogos” until `/api/bible-books` responds.
4. For production, publish the server (`dotnet publish`), copy `web/dist` into `server/wwwroot`, and configure SQLite paths + translation metadata in `appsettings.json`.

Keeping this list tight ensures we can ship the MVP quickly while leaving plenty of seams for future pastoral tooling. Update the checklist + next steps as items land.
