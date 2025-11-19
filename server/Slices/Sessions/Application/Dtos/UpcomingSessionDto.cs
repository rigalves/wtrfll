namespace Wtrfll.Server.Slices.Sessions.Application;

public sealed record UpcomingSessionDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required string ShortCode { get; init; }
    public required string ControllerJoinToken { get; init; }
    public required string DisplayJoinToken { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ScheduledAt { get; init; }
}

