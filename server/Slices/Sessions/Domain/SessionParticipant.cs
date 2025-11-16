namespace Wtrfll.Server.Slices.Sessions.Domain;

public sealed class SessionParticipant
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Session Session { get; set; } = null!;
    public SessionParticipantRole Role { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

