# wtrfll MVP Roadmap (Updated)

wtrfll is a controller + display scripture tool built for a single church LAN. The project uses Vue 3 + Pinia on the frontend and .NET 8 Minimal APIs + SignalR on the backend. This document summarizes the current state (late 2025) and the small set of items still considered part of the MVP.

## Goals

1. **Simple operator experience**: publish scripture references rapidly, preview them, and push to the projector tab in real time.
2. **Shared state**: controller and display stay synchronized through SignalR groups keyed by session.
3. **Open‑source friendly**: clear architecture docs, Scriptural licensing guardrails, tooling for importing translations.
4. **Extendable slices**: vertical-slice server layout (Sessions, Passages, Bibles, BibleBooks) so pastoral tools can be added later without rewrites.

## Current Build

### Frontend
- Vue 3 + Vite + Pinia + Tailwind (typography/forms plugins).
- Router views:
  - `/new` creates a session, displays controller/display links and tokens.
  - `/control/:sessionId?token=` controller view with history, normalized preview, translations list, and publish/reset actions.
  - `/display/:sessionId?token=` display view designed for Chromecast tabs.
- Pinia stores:
  - `bibleBooksStore`: loads canonical book metadata (with spinner overlay and auto retry when backend is offline).
  - `biblesStore`: fetches available translations from `/api/bibles`.
  - `sessionParticipationStore`: handles REST joining, join tokens, role state, and errors.
  - `sessionStore`: parses references, calls `/api/passage`, and pushes/receives realtime `state.patch`/`state.update` payloads.
  - `referenceInputStore`: manages draft inputs and history.
- SignalR client wrapper reconnects automatically and exposes event hooks for the session store.

### Backend
- .NET 8 minimal API project hosting:
  - REST endpoints under `/api` (sessions, passages, bible catalog, bible book metadata).
  - SignalR hub at `/realtime`.
  - SQLite via EF Core for sessions and participants.
- Translation tooling (`tools/Translations`) converts legacy JSON or OSIS/Zefania inputs to normalized JSON stored under `server/Data`.
- Providers (currently RVR1960 + NTV normalized JSON) read from `server/Data/*.normalized.json`.
- Bible book metadata also stored in `server/Data/bibleBooks.metadata.json` and exposed via `/api/bible-books`.
- Vertical slices:
  - `Slices/Sessions`: lifecycle, join tokens, hub authorization, SignalR state fan out.
  - `Slices/Passages`: translation provider interface + normalized JSON provider.
  - `Slices/Bibles`: translation catalog endpoint.
  - `Slices/BibleBooks`: metadata loader for client parser.

## MVP Checklist

| Area | Status | Notes |
|------|--------|-------|
| Session creation & join | ✅ | REST endpoints + Pinia flows complete. |
| Local translations RVR1960 & NTV | ✅ | Normalized JSON via tooling, served from `/api/passage`. |
| Real-time sync (SignalR) | ✅ | Controller publishes patches; display listens to `state.update`. |
| Reference parser | ✅ | Supports Spanish/English aliases, ordinals, ranges. |
| Bible book metadata loading | ✅ | Spinner overlay with auto retry until backend ready. |
| Translation catalog UI | ✅ | `/new` surfaces codes + tokens; controller shows translation list. |
| Chromecast-ready display view | ✅ | Minimal UI with references, verse numbers, and attribution. |
| Offline translation handling | ✅ | Local normalized translations auto-badge as offline ready; controller falls back when online sources fail. |
| PWA / Wake Lock | ✅ | Manifest + service worker + display wake-lock in place. |
| Wake-lock/screen controls | ⏳ | Not implemented. |
| Clear/black/freeze controls | ⏳ | Placeholder for future iteration. |
| Stepping (next/prev) | ✅ | Controller now exposes next/previous verse controls with realtime highlighting. |

## Deferred / Next Wave

These were requested originally but intentionally deferred:

- Setlists and sermon flow builder.
- Stage view, service timers, remote phone control.
- Advanced themes/backgrounds or animations.
- Cloud syncing / multi-tenant features.

## Technology Summary

| Layer | Stack |
|-------|-------|
| Frontend | Vue 3, Vite, TypeScript, Pinia, Vue Router, Tailwind CSS, Headless UI |
| Backend | .NET 8 Minimal APIs, EF Core (SQLite), SignalR |
| Tooling | Node 18+ for Vite, .NET 8 SDK for server/tooling, OpenAPI + `openapi-typescript` for client types |
| Deployment | Single self-hosted server on the LAN serving both static files and APIs; same-origin to avoid CORS |

## Licensing Guardrails

- Only ship translations with public-domain status or explicit permission. RVR1960 and NTV are local normalized copies—do not commit source content beyond derived metadata.
- `tools/Translations` should be used offline; licensing specifics belong in `docs/Licensing.md` (future).
- Display view already renders attribution text sent by the passage provider.

## LAN Deployment Notes

1. Run `dotnet run` in `/server` to start API + SignalR + static files.
2. Run `npm run dev` (or `npm run build` + `npm run preview`) in `/web` for local development; `vite.config.ts` proxies `/api` and `/realtime` to the backend.
3. For self-hosted production, publish the server (`dotnet publish`), copy `web/dist` into `server/wwwroot`, and configure the SQLite file location + bible metadata path in `appsettings.json`.
4. The frontend automatically shows “Cargando catálogos” until `/api/bible-books` responds, so start the backend before launching the browser during services.

## Immediate Next Steps

1. **Stepping controls**: add `next`/`previous` commands in the controller UI that update `currentIndex` within the realtime patch.
2. **Clear/black/freeze**: extend the session state schema to include display commands (clear text, black screen) and reflect them in the display view.
3. **Wake Lock / PWA** (optional for MVP but desired): add Vite PWA plugin and request screen wake lock on the display view.
4. **Basic tests**: add Vitest coverage for the session store’s realtime handling and the parser’s new metadata loading path.

Keeping this list tight ensures the MVP ships quickly while leaving plenty of seams for future pastoral tooling. Feel free to mark items as completed and append new mini-goals as the project evolves.***
