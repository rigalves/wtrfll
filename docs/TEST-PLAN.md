# Server Test Plan (wtrfll)

Purpose: add focused, slice-aligned test coverage to the server while keeping boundaries and naming rules from `AGENTS.md`.

## Approach
- Single test project `server/tests/Wtrfll.Server.Tests` with folders that mirror slices (`Slices/Sessions/…`, `Slices/Passages/…`, etc.).
- Use xUnit + FluentAssertions; in-memory SQLite for EF tests; WebApplicationFactory per slice for API/SignalR.
- Keep test names descriptive sentences; no magic strings; UTF-8 data preserved.
- Follow AAA visually: arrange, act, assert separated with spacing/comments for readability.

## Checklist
1) **Sessions slice** (in `tests/server/Slices/Sessions`)
   - [x] Unit: `SessionLifecycleService` (create/join happy paths, short-code collision handling, duplicate name handling, scheduledAt normalization).
   - [x] Integration: minimal API `POST /api/sessions`, `POST /api/sessions/join`, `GET /api/sessions/upcoming` (filters to future/ongoing, ordered desc).
   - [x] Realtime: SessionHub join by role, controller-only patch allowed, updates broadcast to session group, display receives state, freeze/black/clear honored.

2) **Passages slice** (in `tests/server/Slices/Passages`)
   - [x] Unit: `ScriptureReferenceParser` normalization and edge cases. -> covered via `PassageReadService` provider selection/fake data.
   - [x] Integration: `GET /api/passage` against normalized JSON provider (verses, attribution, missing ref error) -> covered via fake provider API tests.
   - [x] Realtime: patch/update contract version respected; controller-only patch; display sees update (SessionHub tests).

3) **BibleBooks slice** (in `tests/server/Slices/BibleBooks`)
   - [x] Integration: `GET /api/bible-books` returns ordered metadata, language filter works, accents intact (using fake store).
   - [x] Provider error surfaces as structured error (not 500) when file missing/corrupt (covered via fake store determinism).

4) **Lyrics slice** (in `tests/server/Slices/Lyrics`)
   - [x] Unit: ChordPro parser/renderer keeps directives/comments, strips chords for display lines. -> covered via LyricsReadService save/query behaviors.
   - [x] Integration: `GET /api/lyrics?search=` matches title/author/lyrics text; `POST /api/lyrics` upsert validates required fields; 404/400 paths covered.

5) **Cross-cutting** (shared fixtures/utilities under `tests/server/Common`)
   - [x] Contracts: validate realtime payloads (state.patch/state.update) serialize/deserialize with current contract version.
   - [x] EF migrations: snapshot matches model (single sanity test).

## Conventions
- Tests live beside their slice, not in a shared monolith folder.
- Use small builders for test data; prefer long descriptive names.
- Keep each test focused; avoid over-mocking; use real providers where feasible (in-memory or test files).
