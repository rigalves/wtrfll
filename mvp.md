wtrfll is a simple, fast Controller + Display web app tailored for church use. Below is a concise plan with feature ideas and a practical, scalable solution.
Project: wtrfll (MVP Proposal, Vue + .NET)

Goals
- Build a simple, fast Controller + Display app for scripture on a projector.
- Learn modern Vue (Vue 3, Vite, Pinia, PWA) and pragmatic .NET 8 APIs.
- Keep scope tight now; leave clean seams for future pastoral tools.
- Open source friendly: clear structure, docs, and licensing.

What We Will Ship (MVP)
- Two views: `Controller` (operator) and `Display` (projector tab).
- Live sync: Controller selections update Display in real-time.
- Rapid scripture lookup: smart parser (e.g., `jn 3:16-18`, `ps 23`).
- Stepping: next/prev by verse; toggle verse numbers; show reference.
- Visibility controls: clear/black screen; freeze/unfreeze.
- Readability: adjustable font size, high-contrast theme, safe margins.
- PWA installable; wake-lock to keep the screen active; offline-ready for bundled translations.
- Pairing: session code/QR; simple tab casting via Chrome to Chromecast.
 - Translations (defaults, Spanish-first):
   - Preferred defaults: RVR1960 and NTV (configured via API keys due to licensing).

Deferred (leave extension points, not implemented yet)
- Setlists and Sermon Flow builder.
- Stage view and service timers.
- Advanced backgrounds/themes and transitions.
- Remote phone control (browser still works fine).

Stack (Modern Vue + .NET)
- Frontend: Vue 3 + Vite + TypeScript + Pinia + Vue Router + Tailwind CSS + Headless UI (Vue).
- Tailwind plugins: typography, forms, line-clamp. No component framework (no Quasar).
- PWA: Vite PWA plugin; Service Worker caching; Screen Wake Lock API.
- Backend: .NET 8 Minimal APIs + SignalR (WebSockets) for real-time sync.
- Storage (pick one to start; both are open-source and free):
  - SQLite + EF Core (simple, robust, transactional; great for single-host LAN).
  - LiteDB (embedded NoSQL for .NET; very simple; â€œhipâ€ alternative; file-based).
- Packaging: Single self-hosted server on the church LAN (Windows or Linux), serving both API and static frontend for same-origin simplicity.

Architecture
- One server process hosts:
  - Static frontend (Vite build) at `/`.
  - JSON APIs at `/api/*`.
  - SignalR hub at `/realtime` (groups per session).
- Client routes (example):
  - `/display/:sessionId` â†’ Projector view (cast this tab).
  - `/control/:sessionId` â†’ Operator view.
  - `/new` â†’ Creates session, shows QR/code linking Controllerâ†”Display.
- Session pairing: Display shows a session code and QR; Controller joins via code; both join the same SignalR group.

MVP Features in Detail
- Lookup & parsing:
  - Fuzzy book abbreviations (`jn`, `1co`), verse ranges (`3:16-18`), chapters-only (`ps 23`).
  - Suggestion list while typing; Enter to show.
- Display rendering:
  - Step by verse; toggle verse numbers and reference placement (corner/bottom).
  - Adjustable font size; safe area slider for overscan; high contrast theme.
  - Clear/black/freeze controls.
- Sync state:
  - Shared slide state in SignalR group: {translation, passage, currentIndex, theme, options}.
  - Single-controller lock per session (others join as viewers by default).

Licensing (Spanish focus)
- Public domain/offline-safe:
  - Optionally include WEB/KJV as non-defaults for comparison/testing.
- Licensed translations requested as defaults:
  - RVR1960 (Reina-Valera 1960): copyrighted; license required. Do not bundle.
  - NTV (Nueva TraducciÃ³n Viviente): copyrighted (Tyndale); license required. Do not bundle.
- Recommended approach for licensed texts:
  - Configure providers via Settings with API keys (church obtains or uses approved APIs).
  - Fetch verses at runtime; adhere to cache limitations (often no long-term/offline caching).
  - Show required attribution on Display (small corner text) and in Settings/About.
- Repository safeguards:
  - Ship only public-domain text files; never commit licensed content.
  - Document provider setup and attribution requirements clearly in README.

â€œPre-paginate slidesâ€ explained (simplified)
- Purpose: avoid jarring layout shifts mid-service when stepping through a passage.
- Simple MVP approach: break content by verse boundaries ahead of time (each verse is a slide). This is predictable and fast.
- Optional later: measure text on the Display viewport and split long verses into multiple slides ahead of time to guarantee consistent line breaks.

LAN Deployment Considerations
- Single origin: Serve frontend and API from the same server/host to avoid CORS complexity.
  - If you must split origins, enable CORS on the API for `http://<server-ip>:<port>` and `http://localhost:<port>` and allow WebSockets.
- Bind to `0.0.0.0`; open firewall for the chosen port; prefer a static IP or local DNS entry.
- HTTPS: PWAs and some APIs prefer secure contexts. Use `mkcert` to generate a trusted local cert, or use HTTP during early LAN-only testing.
- Private Network Access: Modern browsers may warn on cross-subnet. Keep Controller and Display on the same LAN.
- Chromecast: Casting a Chrome tab containing `/display/:sessionId` is sufficient; no custom Cast receiver needed for MVP.
- Service Worker: Version your cache; provide a clear update strategy (e.g., app shows â€œUpdate availableâ€ prompt).

Security & Roles (MVP-ready, multi-tenant prepared)
- Users: simple email/password or local admin account.
- Roles: `admin`, `presenter`, `viewer` (enforced server-side; only one active controller per session).
- Sessions: include a `church_id` in DB for future multi-tenant separation; UI does not expose it yet.
- Transport: SignalR over WebSockets; emit minimal state; validate server-side.

Data Model (minimal)
- users(id, email, password_hash, name, role, church_id)
- churches(id, name) â€” used internally for future isolation
- sessions(id, church_id, status, current_slide_state_json, created_at)
- themes(id, church_id, name, typography_json, colors_json, backgrounds_json)
- bibles(id, code, language, license, source, version, data_location, provider, cache_policy)
- audit(id, user_id, action, entity, entity_id, timestamp)

Vue App Structure
- `src/main.ts` â€” app bootstrap, Pinia, Router, PWA registration.
- `src/router.ts` â€” routes: `/`, `/new`, `/control/:sessionId`, `/display/:sessionId`.
- `src/stores/session.ts` â€” Pinia store for session + realtime state.
- `src/stores/bible.ts` â€” translation list, passage fetch, cache.
- `src/views/ControlView.vue` â€” controller UI.
- `src/views/DisplayView.vue` â€” projection view.
- `src/components/VerseSearch.vue`, `FontSizeControl.vue`, `VisibilityControls.vue`.
- `src/lib/parseReference.ts` â€” smart reference parser.
- `src/lib/realtime.ts` â€” SignalR client wrapper.
- `public/` â€” fonts, icons, manifest.

.NET 8 Backend Outline
- Minimal APIs with endpoints:
  - `POST /api/sessions` â€” create session (returns id, code, QR payload).
  - `POST /api/sessions/:id/join` â€” join as controller or viewer (locks controller if free).
  - `GET /api/bibles` â€” list available translations, with provider and cache policy.
- SignalR Hub `/realtime`:
  - Group by `sessionId`.
  - Events: `state:patch` (controller), `state:update` (server broadcast), `control:lock`, `heartbeat`.
- Storage layer (choose one):
  - SQLite + EF Core (recommended to start): keep schema minimal, easy backups.
  - LiteDB (NoSQL, embedded): extremely simple; store documents for sessions/themes; good for quick iteration.
 - Provider adapters:
   - Abstraction for translation sources: `LocalText`, `ExternalApi` with per-provider handlers.

UI/UX Tendencies (learning goals folded in)
- Clean, distraction-free layouts; large targets; keyboard-first operation.
- Accessible contrast and scalable typography; poetry-friendly line height.
- Headless components: use Headless UI primitives (Dialog, Combobox, Switch); no Quasar/component framework.
- Minimal motion; instant feedback; undo/last.
- PWA install prompts; offline-first for public-domain bibles.

Project Layout (monorepo)
- `/server` â€” .NET 8 minimal API + SignalR + EF Core/LiteDB.
- `/web` â€” Vue 3 app (Vite, Pinia, PWA).
- `/.github/workflows/` â€” CI: build web + server, run lint/format.
- `/docs` â€” README assets, screenshots, contribution guide.

GitHub Readiness
- MIT license; CONTRIBUTING.md; CODE_OF_CONDUCT.md.
- README: quick start (LAN), screenshots/gifs, feature table, licensing notes, roadmap.
- Issue templates and labels; basic PR checklist.

Implementation Plan
1) Scaffold backend (.NET 8) and hub; serve static web (same origin).
2) Scaffold Vue app with PWA, Router, Pinia; basic views and styling.
5) Wire SignalR: session create/join; group state sync; controller lock.
6) Controls: font size, verse numbers, reference placement, clear/black/freeze.
7) LAN packaging: single binary/container; document mkcert HTTPS option.
8) Docs: README, licensing notes (Spanish focus), provider setup, screenshots; release v0.1.

Notes on Complexity (guardrails)
- Keep CORS out by serving same-origin when possible.
- No custom Cast receiver until needed; cast tab only.
- No setlists/timers yet; leave table placeholders and UI slots.
- Verse-per-slide default; split long verses later only if requested.

Next Decisions
- Storage: start with SQLite + EF Core, or try LiteDB for a NoSQL taste?
- Do you want me to scaffold the repo (Vue + .NET skeleton) next?

Vision

A snappy, church-friendly website with two views: a Controller on your device and a Display thatâ€™s cast to the projector.
Optimized for live use: zero-friction verse lookup, setlists, clear/black screen, and instant transitions.
MVP Features

Quick verse search: type â€œjn 3:16â€“18â€ or â€œps 23â€ with book abbreviations.
Controller + Display: open two URLs; cast the Display tab to Chromecast.
Live sync: any selection in Controller appears instantly on Display.
Slide tools: next/previous, show/hide verse numbers, show reference, clear/black, freeze.
Readability: auto line-breaking, adjustable font size, high-contrast themes.
Backgrounds: solid color/gradient; optional blurred image behind text.
Setlists: pre-build sequences (e.g., sermon flow); quick jump by section.
Undo/History: jump back to the last verse or slide.
PWA installable app: works fullscreen, keeps screen awake during service.
Phase 2 Enhancements

Dual-language projection (e.g., English + Spanish).
Stage display: timer, next item, private notes for preacher/band.
Countdown + service timers; warnings at thresholds (e.g., 5 min left).
Announcements mode with carousel; QR codes (giving, connect, event).
Verse pinning and favorites; â€œcommon versesâ€ packs.
Themes: preset typography + spacing tuned for scripture (poetry, OT quotes).
Transitions: crossfade, fade-through-black, slide-by-verse stepping.
Remote control on phone: minimal UI for next/prev, clear/black.
Collaborative prep: share setlists with roles (admin/presenter).
Bilingual controller notes; transliteration or romanization overlay.
Nursery call/stage messages (only visible on stage display).
Import/export setlists (JSON); Planning Center import.
Song lyrics mode with CCLI reporting (optional).
Pastoral Tools

Sermon flow builder: sections (Welcome, Reading, Sermon, Communion, Announcements).
Notes alongside slides: key points, prayer prompts; only on Controller/Stage view.
Scripture packs for topics (hope, grief, giving, communion).
Prayer mode: rotate requests, toggle anonymity.
Quick â€œverses by topicâ€ search with curated references.
Follow-up resources: auto-generate a take-home reading plan QR.
Bible Text Sources and Licensing

Public domain/open: KJV, World English Bible (WEB), Louis Segond 1910 (FR), etc.
Licensed APIs: ESV (Crossway), NIV, NLT, NKJV require agreements/keys.
MVP approach: bundle WEB + KJV for offline; allow optional API keys per translation. Show license info in settings and on Display if required.
Architecture Options

Simple MVP (fastest):
Frontend-only PWA (e.g., SvelteKit or Next.js/React).
Real-time sync via hosted service (Supabase Realtime or Firebase).
Bible text stored locally (IndexedDB) for WEB/KJV; optional API fetch for others.
Scalable variant:
Lightweight Node server (Express/Fastify) for WebSocket sync + media proxy.
Optional Google Cast receiver app for low-latency casting and reliability.
Recommendation: Start with a frontend-only PWA + Supabase Realtime. Add Node/WebSocket or Google Cast receiver later if needed.

Key Components

Controller view
Search bar with smart parsing (â€œjnâ€, â€œ1coâ€, ranges).
Book/Chapter navigator; fuzzy abbreviations.
Setlist sidebar; â€œnow/nextâ€ with drag-reorder.
Controls: next/prev verse, step within passage, clear/black/freeze, theme, font size.
Timers and service time.
Display view
Full-bleed, text-centered content with safe margins.
Verse numbers toggle; reference placement (corner/bottom).
Background (color/gradient/image) and smooth transitions.
Offline fonts for non-Latin scripts (Noto).
Stage view (Phase 2)
Big clock, countdowns, â€œNextâ€ content, private notes, stage messages.
Sync and Casting

Pairing: Display opens yourapp.com/display/:sessionId showing a QR; Controller scans to join.
Real-time: broadcast the â€œcurrent slide stateâ€ to the session channel.
Casting MVP: Chromeâ€™s â€œCast tabâ€ to Chromecast for Display.
Upgrade path: Google Cast Sender + custom Receiver app for resilient casting.
Offline and Performance

PWA with service worker caching for app shell + Bible data (WEB/KJV zipped JSON).
IndexedDB for translations, themes, setlists; background sync when online.
Pre-paginate slides for a passage to avoid layout jank during live stepping.
Use GPU-accelerated transitions; aim for 60fps.
Security and Roles

Auth: email/password + magic links.
Roles: admin, presenter, viewer.
Sessions: one active controller per session; prevent control conflicts.
Multi-tenant: â€œchurchâ€ isolation if you host for others.
Data Model (minimal)

users(id, name, role)
churches(id, name)
sessions(id, church_id, status, current_slide_state, theme_id)
setlists(id, church_id, name, items[])
themes(id, church_id, name, typography, colors, backgrounds)
media(id, church_id, url, type, meta)
Local IndexedDB: bibles(version, language, books, index)
Tech Stack

UI: SvelteKit (fast, minimal) or Next.js + Tailwind for speed.
State: Zustand/Redux (React) or Svelte stores.
Realtime: Supabase Realtime channels.
Storage: Supabase Storage or S3 for backgrounds.
PWA: Workbox for service worker, Wake Lock API to keep screen on.
Fonts: Noto families; ship preloaded font subsets.
Implementation Plan

Week 1: MVP
SvelteKit/Next app scaffold; PWA + Wake Lock.
Local WEB/KJV; reference parser; search by book/chapter/verse.
Controller + Display routes; session QR pairing; Supabase channel sync.
Basic theming, font sizing, clear/black, next/prev within a passage.
Week 2: Polish
Setlists; drag-and-drop; history/undo.
Verse number toggle; reference formatting; smooth crossfade.
Safe-margin guides; default templates; mobile remote layout.
Week 3+: Phase 2
Dual-language display; stage view with notes/timers.
Countdown clocks; announcement mode with QR cards.
Optional: Google Cast receiver app for robust casting.
Risks and Mitigations

Translation licensing: default to WEB/KJV; prompt for API keys for others; show attribution.
Latency on poor Wi-Fi: pre-paginate; cache locally; consider Cast receiver or local WebSocket server.
Projector overscan: adjustable safe area and zoom; live preview grid.
Quick MVP Flow

Open yourapp.com/display/new â†’ shows QR and session code.
On yourapp.com/control, join session via code or scan QR.
Type â€œjohn 3:16-18â€ â†’ paginated slides appear.
Click â€œShowâ€ â†’ casted Display updates instantly; use next/prev and clear/black.
