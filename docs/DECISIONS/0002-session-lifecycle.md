ADR 0002: Session Lifecycle and Join Flow

Status
- Accepted

Context
- Controller/Display currently rely on hard-coded demo session ids. We need persistent sessions with join codes so multiple operators can run services on LAN, and so SignalR groups can be keyed by server-generated ids.

Decision
- Store sessions in SQLite via EF Core with the schema:
  - Sessions(Id GUID PK, ShortCode string UNIQUE, CreatedAt, Status enum, ControllerJoinToken, DisplayJoinToken)
  - SessionParticipants(Id GUID PK, SessionId FK, Role enum, JoinedAt)
- Expose REST endpoints:
  - POST /api/sessions -> creates a session, returns { id, shortCode, controllerJoinToken, displayJoinToken }.
  - POST /api/sessions/{id}/join -> body { joinToken, role }, validates token, records participant, returns { ok, sessionId, role }.
- Frontend flow:
  - /new calls create-session, shows short code and copyable URLs.
  - /control/:sessionId and /display/:sessionId call join endpoint at mount, storing returned metadata in Pinia; SignalR will use the same session id + role to join groups.

Consequences
- Enables multiple concurrent sessions and future persistence of session state.
- Introduces migrations and contract changes. Need to regenerate TS types.
