# wtrfll Roadmap (Q4 2025)

Short list of items still in motion for the MVP. Update the checkboxes as tasks ship.

## ✅ Done
- SignalR end-to-end wiring (SessionHub + Pinia realtime client).
- Bible book metadata loader + retry overlay.
- Session creation/join flow with named sessions, scheduled times, and short codes.
- Landing role selector with upcoming sessions and deep links.
- Translation tooling + normalized JSON providers (RVR1960, NTV).
- Pinia refactor into vertical stores (participation, session, bible books, upcoming sessions).
- Offline translation handling (catalog metadata + controller fallback notice).
- Stepping controls + realtime verse highlighting.
- Display commands (clear/black/freeze) + controller badges.
- PWA basics (manifest + service worker) and wake-lock on display view.

## ⏳ In Flight / Next Up
1. **Session metadata surfacing**
   - Show the friendly session name + scheduled time in controller and display headers.
   - Reuse the metadata already stored in `sessionParticipationStore`.
2. **Basic test coverage**
   - Vitest suites for `sessionStore` realtime handling and loaders (`bibleBooksStore`, `upcomingSessionsStore`).
3. **Share helpers**
   - QR codes or OS share sheets for controller/display links so projector laptops can join faster.

## Deferred (Post-MVP)
- Setlists + sermon flow builder.
- Stage view & timers.
- Advanced themes/backgrounds/animations.
- Remote phone control / multi-device operators.

Keep the roadmap short and focused; each bullet should translate to one or two commits.
