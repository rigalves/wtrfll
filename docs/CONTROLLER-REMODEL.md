# Controller Remodel Plan (Late 2025)

## Vision

Move the controller page from a single “scripture reference” panel to a ProPresenter/OBS-style workspace that can manage multiple slide types (scripture passages, lyrics, images) while keeping the existing vertical-slice architecture and SignalR flow.

## Phased Plan

### Phase 1 — Layout & Slide List
- Convert the controller view to a 3-column layout:
  - Header: session info, join status, quick commands, translation selector.
  - Left Sidebar: prepared slides & media library.
  - Center: dynamic editor (only the active slide’s panel is rendered) + live preview.
  - Right Drawer: styling controls (font, colors) — placeholder for now.
- Introduce `useSessionSlidesStore` responsible for:
  - Maintaining `slides: Array<{ id, type, label, payload }>` for the session.
  - Actions `addSlide`, `updateSlide`, `setActiveSlide`, `removeSlide`.
  - Using existing session lifecycle service for persistence later.
- UI tasks:
  - Auto-save slide edits (no extra “apply” step) while keeping Publish button global.
  - Scripture stepping buttons continue to publish immediately.
  - Sidebar list with icons, reorder later; for now simple list + delete icons.
  - Preview component mirrors the live display (reuse DisplayView layout) so volunteers always see what the projector shows.
- Deliverable: slides exist client-side only; publishing still pushes full passage payload from editor.

### Phase 2 — Lyrics Model & Library
- DB Schema (lightweight) for lyrics using JSON/ChorPro-inspired format.
  - Example payload stored server-side: `{ id, title, language, sections: [{ code, type, lines }] }`.
- API endpoints for listing/storing lyrics to attach to a session (initially read-only from seed data).
- UI:
  - Sidebar “Lyrics Library” section showing available songs with an autocomplete selector.
  - Selecting a lyric fills a large template-aware textarea (ChordPro-like) for final edits.
- Branding touchpoints:
  - Ensure the new library UI uses the wtrfll iconography/branding (icons already in `assets/icons/`).
  - Display the app emblem near library headers when the drawer/section opens.
- Real-time: When publishing a lyrics slide, `SessionStatePatchPayload` includes `slideType: 'lyrics'` and `payload: { songId, sectionCode }`.
- Display view renders lyrics layout when slide type is `lyrics`.

### Phase 3 — Images & Media Upload (Future)
- File storage (local disk or S3) plus metadata table.
- Sidebar “Images” shows thumbnail grid.
- Controller preview switches to simple image layout.
- Service worker caches selected images for offline projector.

### Phase 4 — Styling Controls & Presets
- Implement font size, color schemes, layout toggles stored per session slide.
- Styling drawer controls produce a `presentationStyle` object stored alongside slide.
- Display view applies style overrides (colors, fonts) from slide payload.

## Documentation & Tracking

- This doc (docs/CONTROLLER-REMODEL.md) tracks completed phases and any deviations.
- UI-FLOW.md should be updated after each phase to reflect new components/stores.
- For each task:
  - Update this doc with status.
  - Link PR/commit references if possible.
  - Capture open questions (e.g., lyrics import formats).

## Current Status (2025-11-21)
- Phase 1: controller layout + slide list implemented (multi-column UI, slide store, preview panel).
- Phase 2 (lyrics) first cut shipped:
  - Lyrics tables/API + provider wired through the passages service so SignalR keeps the server authoritative.
  - Controller view now lets volunteers search the lyrics catalog, pick sections, and publish them with one click (Publish button detects the slide type).
  - Display view reuses the same SignalR payload, hiding verse numbers when lyrics are showing.
- Remaining Phase 2 tasks: CRUD UI for managing the catalog, expose lyrics presets in docs, and keep the ChordPro-parsing helpers isolated (both server + web) so we can swap lyric formats later without touching the rest of the slices.

## Preview Rules
- Draft cards (Scripture draft / Lyrics draft) always show the current draft for the active slide. They update as you type or change selections; no publish is needed.
- Live preview shows what would appear on the display if you published now, including the active display command (normal/black/clear/freeze).
- Publish sends the active draft to the display; the live preview already matches the draft, so it does not jump on publish.
- The live preview includes a badge for the current display command; black/clear/freeze override content for that preview.
