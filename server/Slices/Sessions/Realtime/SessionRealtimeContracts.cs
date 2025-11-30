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
    public string? DisplayCommand { get; init; }
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
    public string DisplayCommand { get; init; } = SessionDisplayCommands.Normal;
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

public sealed record LyricsStatePatchMessage
{
    public required int ContractVersion { get; init; }
    public required Guid SessionId { get; init; }
    public required LyricsStatePatchBody Patch { get; init; }
}

public sealed record LyricsStatePatchBody
{
    public Guid? LyricsId { get; init; }
    public string? Title { get; init; }
    public string? Author { get; init; }
    public string LyricsChordPro { get; init; } = string.Empty;
    public double? FontScale { get; init; }
}

public sealed record LyricsStateUpdateMessage
{
    public required int ContractVersion { get; init; }
    public required Guid SessionId { get; init; }
    public required LyricsStatePayload State { get; init; }
}

public sealed record LyricsStatePayload
{
    public Guid? LyricsId { get; init; }
    public string? Title { get; init; }
    public string? Author { get; init; }
    public IReadOnlyList<string> Lines { get; init; } = Array.Empty<string>();
    public double? FontScale { get; init; }
}

public static class SessionDisplayCommands
{
    public const string Normal = "normal";
    public const string Black = "black";
    public const string Clear = "clear";
    public const string Freeze = "freeze";

    public static bool IsValid(string? command)
    {
        return command is null or Normal or Black or Clear or Freeze;
    }
}
