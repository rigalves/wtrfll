namespace Wtrfll.Server.Slices.Sessions.Application;

public sealed record CreateSessionRequestDto
{
    public string? Name { get; init; }
    public DateTime? ScheduledAt { get; init; }
}

