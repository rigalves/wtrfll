# wtrfll UI Flow Guide

This guide explains how the Vue + Pinia pieces collaborate so you always know which store owns a concern, how data moves between tabs, and where to extend the system.

## High-Level Layers

1. **Pinia stores** hold all long-lived state. Views import them via `useXYZStore()`.
2. **Views / components** (ControllerView, DisplayView, Landing, etc.) watch store refs through `storeToRefs()` and trigger store actions on user input.
3. **Utility libs** (SignalR client, OpenAPI client, reference parser) keep I/O concerns outside components. Stores call these helpers.
4. **PWA shell + Service Worker** keeps `/`, `/control`, and `/display` navigations fast (pre-cached HTML + manifest) and allows offline-ready translations to render even if the network flickers.

## Store Summary

| Store | Responsibility | Key State / Actions |
|-------|----------------|---------------------|
| `useBibleBooksStore` | Loads canonical bible book metadata and exposes alias helpers for the parser. | `books`, `loadState`, `loadError`, `ensureLoaded()` |
| `useUpcomingSessionsStore` | Calls `/api/sessions/upcoming`, keeps only today/future sessions, sorts them newest-first, and feeds the landing Controller/Display cards. | `orderedSessions`, `groupedByDate`, `isLoading`, `loadErrorMessageKey`, `registerCreatedSession()` |
| `useReferenceInputStore` | Tracks the controller's draft input, published history, and normalization helpers. | `draftInput`, `currentInput`, `recentHistory`, `publishDraftInput()` |
| `useSessionParticipationStore` | Joins sessions via REST, stores join tokens, role, and the friendly session metadata returned by the server. | `activeSessionId`, `activeRole`, `activeSessionName`, `joinState`, `joinErrorMessage`, `joinSession()` |
| `useSessionStore` | Parses scripture references, fetches passages, and pipes SignalR patches/updates between controller and display. | `controllerViewModel`, `displayViewModel`, `publishDraftToSession()`, `normalizeAndLoad()`, `setDisplayCommand()` |
| `useBiblesStore` | Fetches `/api/bibles` for the controller sidebar and annotates offline-ready translations. | `translations`, `loading`, `errorMessage`, `findByCode()` |

## Startup & Global Flow

1. `main.ts` bootstraps Vue, installs Pinia + Router, mounts `<App>`, and immediately kicks off `useBibleBooksStore().ensureLoaded()`.
2. `<App>` renders the “Preparing verse references…” overlay until the bible-book store reports `loadState === 'loaded'`. The store auto-retries when the backend is offline, so operators simply wait for the overlay to disappear.
3. The service worker (`web/public/sw.js`) pre-caches `/`, `/index.html`, and `manifest.webmanifest` so the PWA opens instantly and can load translations even when the network is flaky. Non-core requests bypass the cache so dev tooling (Vite, HMR) stays accurate.
4. Each view imports whichever stores it needs; because Pinia provides singletons, state stays synchronized across routes.

## Landing + Role Selection

1. The landing view (`/`) imports `useUpcomingSessionsStore`, calls `ensureLoaded()`, and renders two cards (Controller + Display). Each card lists today/future sessions (newest-first) and shows the friendly name, short code, and schedule in the browser’s local time.
2. Buttons on each card trigger `router.push({ name: 'control' | 'display', params: { sessionId }, query: { token } })`, so the correct join token flows straight into the URL. Volunteers no longer copy/paste links.
3. The `/new` route lets operators enter a session name and optional schedule. After a successful POST, it calls `upcomingSessionsStore.registerCreatedSession()` so the landing page reflects the new session instantly.
4. Because tokens + session metadata are stored in `localStorage`, refreshing or opening a new tab resumes the session automatically.

## Joining a Session

1. When `/control/:sessionId` or `/display/:sessionId` loads, the view reads the `token` query string and calls `useSessionParticipationStore().joinSession({ sessionId, joinToken, role })`.
2. The participation store POSTs to `/api/sessions/{id}/join`, persists the token (per role) in `localStorage`, and stores the friendly `name` / `scheduledAt` returned by the backend. If the controller already joined, it reports an error so the UI can warn the user.
3. Once `joinState === 'joined'`, other stores (session store, etc.) react via `watch()` to initialize realtime connections or load data.
4. On the display side, the top app chrome disappears (`App.vue` checks `route.name !== 'display'`) so the PWA renders a clean projector surface with a subtle session badge.

## Publishing a Passage (Controller)

1. Controller types a reference and presses **Publicar**. `ControllerView.publishReference()` delegates to `useSessionStore().publishDraftToSession()`.
2. The session store asks `useReferenceInputStore` to publish the draft (update history) and then runs `normalizeAndLoad()`.
3. `normalizeAndLoad()` parses the reference. When valid it updates `currentPassage`, resets `currentPresentationIndex`, and fires `loadPassageContent()` to call `/api/passage`. It also calls `broadcastStatePatch()` so displays receive the normalized reference immediately.
4. Stepping controls call `sessionStore.stepToPreviousVerse()` / `stepToNextVerse()`, which update `currentPresentationIndex` and emit another realtime patch so displays highlight the right verse without waiting for a fresh REST response.
5. `setDisplayCommand()` triggers black/clear/freeze states. Commands are part of the SignalR payload, so both controller and display view models update instantly.

## Display Flow

1. After the participation store confirms a display join, `useSessionStore` creates a `SessionRealtimeClient`, subscribes to `state:update`, and fetches the latest passage if needed.
2. `DisplayView` renders `displayViewModel`, which includes reference, verses, attribution, active verse index, and display command.
3. The display view requests a wake lock once joined so projectors or Chromecast tabs remain awake during the service.

## Error Handling

- Participation errors (`session.joinErrors.*`) surface as banners in controller and display views.
- Passage load failures set `controllerViewModel.passageErrorMessage`. The controller view binds to it and shows actionable copy.
- `useSessionStore` emits `translationNotice` when an online translation fails and it falls back to an offline-ready translation; the controller view renders this notice near the reference form.
- `<App>` keeps retrying the bible book catalog request so tech volunteers simply wait for the overlay to clear instead of reloading manually.

## Extending the Flow

- Add new long-lived concerns as Pinia stores inside the relevant slice folder (mirroring the backend vertical slices).
- Resist calling REST/SignalR directly from components. Keep I/O inside stores so state transitions remain predictable.
- Use descriptive names for store actions (per AGENTS.md). Shortcuts like `doStuff()` make it harder to trace flows later.

With this map you can trace how data travels from the landing role selector into controller/display tabs, then down to the server and back via SignalR. Update the doc whenever a new slice introduces additional stores or flows.
