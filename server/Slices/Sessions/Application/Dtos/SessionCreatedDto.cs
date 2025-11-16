namespace Wtrfll.Server.Slices.Sessions.Application;

public sealed record SessionCreatedDto
{
    public Guid Id { get; init; }
    public required string ShortCode { get; init; }
    public required string ControllerJoinToken { get; init; }
    public required string DisplayJoinToken { get; init; }
    public DateTime CreatedAt { get; init; }
}

