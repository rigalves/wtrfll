namespace Wtrfll.Server.Slices.Sessions.Domain;

public sealed class Session
{
    public Guid Id { get; set; }
    public string ShortCode { get; set; } = string.Empty;
    public string ControllerJoinCode { get; set; } = string.Empty;
    public string DisplayJoinCode { get; set; } = string.Empty;
    public SessionStatus Status { get; set; } = SessionStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<SessionParticipant> Participants { get; set; } = new();
}

