using Wtrfll.Server.Slices.Sessions.Domain;

namespace Wtrfll.Server.Slices.Sessions.Application;

public sealed record JoinSessionResultDto
{
    public bool Ok { get; init; }
    public bool ControllerLocked { get; init; }
    public string? Message { get; init; }
    public Guid SessionId { get; init; }
    public required string ShortCode { get; init; }
    public SessionParticipantRole Role { get; init; }
}


