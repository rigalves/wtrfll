using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Wtrfll.Server.Slices.Passages.Application;
using Wtrfll.Server.Slices.Sessions.Application;
using Wtrfll.Server.Slices.Sessions.Domain;
using Wtrfll.Server.Slices.Lyrics.Application;

namespace Wtrfll.Server.Slices.Sessions.Realtime;

public sealed class SessionHub : Hub
{
    private readonly SessionLifecycleService _sessionLifecycleService;
    private readonly PassageReadService _passageReadService;
    private readonly SessionConnectionRegistry _connectionRegistry;
    private readonly LyricsPresentationService _lyricsPresentationService;
    private readonly ILogger<SessionHub> _logger;
    private readonly ConcurrentDictionary<Guid, string> _displayCommands = new();

    public SessionHub(
        SessionLifecycleService sessionLifecycleService,
        PassageReadService passageReadService,
        SessionConnectionRegistry connectionRegistry,
        LyricsPresentationService lyricsPresentationService,
        ILogger<SessionHub> logger)
    {
        _sessionLifecycleService = sessionLifecycleService;
        _passageReadService = passageReadService;
        _connectionRegistry = connectionRegistry;
        _lyricsPresentationService = lyricsPresentationService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext is null)
        {
            _logger.LogWarning("Connection {ConnectionId} rejected: missing HTTP context.", Context.ConnectionId);
            Context.Abort();
            return;
        }

        var sessionIdValue = httpContext.Request.Query["sessionId"].ToString();
        var roleValue = httpContext.Request.Query["role"].ToString();
        var joinTokenValue = httpContext.Request.Query["joinToken"].ToString();

        if (!Guid.TryParse(sessionIdValue, out var sessionId) ||
            string.IsNullOrWhiteSpace(roleValue) ||
            string.IsNullOrWhiteSpace(joinTokenValue))
        {
            _logger.LogWarning("Connection {ConnectionId} rejected: missing query params.", Context.ConnectionId);
            Context.Abort();
            return;
        }

        if (!Enum.TryParse<SessionParticipantRole>(roleValue, ignoreCase: true, out var role))
        {
            _logger.LogWarning("Connection {ConnectionId} rejected: invalid role {Role}.", Context.ConnectionId, roleValue);
            Context.Abort();
            return;
        }

        var tokenValid = await _sessionLifecycleService.ValidateJoinTokenAsync(
            sessionId,
            role,
            joinTokenValue,
            Context.ConnectionAborted);

        if (!tokenValid)
        {
            _logger.LogWarning("Connection {ConnectionId} rejected: invalid join token for session {SessionId}.", Context.ConnectionId, sessionId);
            Context.Abort();
            return;
        }

        var connectionContext = new SessionConnectionContext(sessionId, role);
        if (!_connectionRegistry.TryRegister(Context.ConnectionId, connectionContext))
        {
            _logger.LogWarning("Connection {ConnectionId} rejected: duplicate registration attempt.", Context.ConnectionId);
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, connectionContext.GroupName);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_connectionRegistry.TryRemove(Context.ConnectionId, out var connectionContext) && connectionContext is not null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, connectionContext.GroupName);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task StatePatch(SessionStatePatchMessage message)
    {
        if (!_connectionRegistry.TryGet(Context.ConnectionId, out var connectionContext) || connectionContext is null)
        {
            throw new HubException("Connection is not associated with a session.");
        }

        if (!connectionContext.IsController)
        {
            throw new HubException("Only controllers can publish state patches.");
        }

        if (message.ContractVersion != SessionRealtimeContracts.ContractVersion)
        {
            throw new HubException("Unsupported contract version.");
        }

        if (message.SessionId != connectionContext.SessionId)
        {
            throw new HubException("Session mismatch.");
        }

        if (message.Patch is null ||
            string.IsNullOrWhiteSpace(message.Patch.Translation) ||
            string.IsNullOrWhiteSpace(message.Patch.PassageRef))
        {
            throw new HubException("Patch payload is incomplete.");
        }

        var passage = await _passageReadService.GetPassageAsync(
            message.Patch.Translation,
            message.Patch.PassageRef,
            Context.ConnectionAborted);

        if (passage is null)
        {
            throw new HubException("Translation or reference not available.");
        }

        var command = ResolveDisplayCommand(message.SessionId, message.Patch.DisplayCommand);

        var update = new SessionStateUpdateMessage
        {
            ContractVersion = SessionRealtimeContracts.ContractVersion,
            SessionId = message.SessionId,
            State = new SessionRealtimeState
            {
                Translation = passage.Translation,
                Reference = passage.Reference,
                Verses = passage.Verses,
                Attribution = passage.Attribution,
                Options = message.Patch.Options,
                CurrentIndex = message.Patch.CurrentIndex,
                DisplayCommand = command,
            },
        };

        await Clients.Group(connectionContext.GroupName)
            .SendAsync("state:update", update, Context.ConnectionAborted);
    }

    public Task Heartbeat(SessionHeartbeatRequest? request = null)
    {
        if (!_connectionRegistry.TryGet(Context.ConnectionId, out var connectionContext) || connectionContext is null)
        {
            throw new HubException("Connection is not associated with a session.");
        }

        var response = new SessionHeartbeatMessage
        {
            ContractVersion = SessionRealtimeContracts.ContractVersion,
            SessionId = connectionContext.SessionId,
            Role = connectionContext.Role.ToString().ToLowerInvariant(),
            ServerTimestamp = DateTimeOffset.UtcNow,
        };

        return Clients.Caller.SendAsync("heartbeat", response, Context.ConnectionAborted);
    }

    public async Task PublishLyrics(LyricsStatePatchMessage message)
    {
        if (!_connectionRegistry.TryGet(Context.ConnectionId, out var connectionContext) || connectionContext is null)
        {
            throw new HubException("Connection is not associated with a session.");
        }

        if (!connectionContext.IsController)
        {
            throw new HubException("Only controllers can publish lyrics.");
        }

        if (message.ContractVersion != SessionRealtimeContracts.ContractVersion)
        {
            throw new HubException("Unsupported contract version.");
        }

        if (message.SessionId != connectionContext.SessionId)
        {
            throw new HubException("Session mismatch.");
        }

        var payload = await _lyricsPresentationService.BuildLyricsStateAsync(message.Patch, Context.ConnectionAborted);

        var update = new LyricsStateUpdateMessage
        {
            ContractVersion = SessionRealtimeContracts.ContractVersion,
            SessionId = message.SessionId,
            State = payload,
        };

        await Clients.Group(connectionContext.GroupName)
            .SendAsync("lyrics:update", update, Context.ConnectionAborted);
    }

    private string ResolveDisplayCommand(Guid sessionId, string? requestedCommand)
    {
        if (!string.IsNullOrWhiteSpace(requestedCommand))
        {
            if (!SessionDisplayCommands.IsValid(requestedCommand))
            {
                throw new HubException("Invalid display command.");
            }

            _displayCommands.AddOrUpdate(sessionId, requestedCommand, (_, _) => requestedCommand);
            return requestedCommand;
        }

        if (_displayCommands.TryGetValue(sessionId, out var existing))
        {
            return existing;
        }

        _displayCommands[sessionId] = SessionDisplayCommands.Normal;
        return SessionDisplayCommands.Normal;
    }
}
