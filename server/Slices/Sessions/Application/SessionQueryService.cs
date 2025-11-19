using Microsoft.EntityFrameworkCore;
using Wtrfll.Server.Infrastructure.Data;

namespace Wtrfll.Server.Slices.Sessions.Application;

public sealed class SessionQueryService
{
    private readonly AppDbContext _dbContext;

    public SessionQueryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<UpcomingSessionDto>> GetUpcomingSessionsAsync(
        DateTime referenceUtc,
        CancellationToken cancellationToken)
    {
        var graceWindowStart = referenceUtc.AddDays(-7);

        var sessions = await _dbContext.Sessions
            .AsNoTracking()
            .Where(session =>
                (session.ScheduledAt != null && session.ScheduledAt >= graceWindowStart) ||
                (session.ScheduledAt == null && session.CreatedAt >= graceWindowStart))
            .OrderBy(session => session.ScheduledAt ?? session.CreatedAt)
            .Take(50)
            .Select(session => new UpcomingSessionDto
            {
                Id = session.Id,
                Name = session.Name,
                ShortCode = session.ShortCode,
                ControllerJoinToken = session.ControllerJoinCode,
                DisplayJoinToken = session.DisplayJoinCode,
                CreatedAt = session.CreatedAt,
                ScheduledAt = session.ScheduledAt,
            })
            .ToListAsync(cancellationToken);

        return sessions;
    }
}
