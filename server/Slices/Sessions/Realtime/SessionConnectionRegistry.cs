using System.Collections.Concurrent;
using Wtrfll.Server.Slices.Sessions.Domain;

namespace Wtrfll.Server.Slices.Sessions.Realtime;

public sealed class SessionConnectionRegistry
{
    private readonly ConcurrentDictionary<string, SessionConnectionContext> _connections = new();

    public bool TryRegister(string connectionId, SessionConnectionContext context)
    {
        return _connections.TryAdd(connectionId, context);
    }

    public bool TryGet(string connectionId, out SessionConnectionContext? context)
    {
        return _connections.TryGetValue(connectionId, out context);
    }

    public bool TryRemove(string connectionId, out SessionConnectionContext? context)
    {
        return _connections.TryRemove(connectionId, out context);
    }
}

public sealed record SessionConnectionContext(Guid SessionId, SessionParticipantRole Role)
{
    public string GroupName => $"session:{SessionId:N}";
    public bool IsController => Role == SessionParticipantRole.Controller;
}
