using Wtrfll.Server.Slices.Passages.Application;

namespace Wtrfll.Server.Slices.Sessions.Realtime;

public static class SessionRealtimeContracts
{
    public const int ContractVersion = 1;
}

public sealed record SessionStatePatchMessage
{
    public required int ContractVersion { get; init; }
    public required Guid SessionId { get; init; }
    public required SessionStatePatchBody Patch { get; init; }
}

public sealed record SessionStatePatchBody
{
    public required string Translation { get; init; }
    public required string PassageRef { get; init; }
    public int CurrentIndex { get; init; }
    public SessionPresentationOptions? Options { get; init; }
}

public sealed record SessionStateUpdateMessage
{
    public required int ContractVersion { get; init; }
    public required Guid SessionId { get; init; }
    public required SessionRealtimeState State { get; init; }
}

public sealed record SessionRealtimeState
{
    public required string Translation { get; init; }
    public required string Reference { get; init; }
    public required IReadOnlyList<VerseDto> Verses { get; init; }
    public required int CurrentIndex { get; init; }
    public SessionPresentationOptions? Options { get; init; }
    public AttributionDto? Attribution { get; init; }
}

public sealed record SessionPresentationOptions
{
    public bool? ShowVerseNumbers { get; init; }
    public bool? ShowReference { get; init; }
    public double? FontScale { get; init; }
    public double? SafeMarginPct { get; init; }
    public string? Theme { get; init; }
}

public sealed record SessionHeartbeatMessage
{
    public required int ContractVersion { get; init; }
    public required Guid SessionId { get; init; }
    public required string Role { get; init; }
    public required DateTimeOffset ServerTimestamp { get; init; }
}

public sealed record SessionHeartbeatRequest
{
    public required int ContractVersion { get; init; }
    public required Guid SessionId { get; init; }
}
