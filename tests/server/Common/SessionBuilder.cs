using Wtrfll.Server.Slices.Sessions.Application;

namespace Wtrfll.Server.Tests.Common;

/// <summary>
/// Builder to produce consistent session requests for tests with readable defaults.
/// </summary>
public sealed class SessionBuilder
{
    private string _name = "Test Session";
    private DateTime? _scheduledAt;

    public static SessionBuilder Create() => new();

    public SessionBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public SessionBuilder ScheduledAt(DateTime? scheduledAt)
    {
        _scheduledAt = scheduledAt;
        return this;
    }

    public CreateSessionRequestDto BuildRequest() => new()
    {
        Name = _name,
        ScheduledAt = _scheduledAt,
    };
}
