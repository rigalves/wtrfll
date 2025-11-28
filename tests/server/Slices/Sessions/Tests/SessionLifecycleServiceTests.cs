using FluentAssertions;
using Wtrfll.Server.Slices.Sessions.Application;
using Wtrfll.Server.Slices.Sessions.Domain;
using Wtrfll.Server.Tests.Common;
using Xunit;

namespace Wtrfll.Server.Tests.Slices.Sessions;

public sealed class SessionLifecycleServiceTests
{
    [Fact]
    public async Task CreateSession_uses_fallback_name_and_normalizes_scheduled_at_to_utc()
    {
        await using var db = await SqliteTestDb.CreateAsync();
        var service = new SessionLifecycleService(db.DbContext);
        var localTime = DateTime.SpecifyKind(new DateTime(2025, 1, 1, 9, 30, 0), DateTimeKind.Local);
        var request = SessionBuilder.Create()
            .WithName("   ")
            .ScheduledAt(localTime)
            .BuildRequest();

        // Act
        var result = await service.CreateSessionAsync(request, CancellationToken.None);

        // Assert
        result.Name.Should().StartWith("Session ");
        result.ScheduledAt.Should().NotBeNull();
        result.ScheduledAt!.Value.Kind.Should().Be(DateTimeKind.Utc);
        db.DbContext.Sessions.Should().ContainSingle();
        db.DbContext.Sessions.Single().ShortCode.Should().HaveLength(6);
    }

    [Fact]
    public async Task JoinSession_blocks_second_controller_and_returns_locked()
    {
        await using var db = await SqliteTestDb.CreateAsync();
        var service = new SessionLifecycleService(db.DbContext);
        var created = await service.CreateSessionAsync(SessionBuilder.Create().WithName("Sunday").BuildRequest(), CancellationToken.None);

        // Act
        var firstJoin = await service.JoinSessionAsync(created.Id, SessionParticipantRole.Controller, created.ControllerJoinToken, CancellationToken.None);
        firstJoin.Status.Should().Be(JoinSessionOperationStatus.Success);

        var secondJoin = await service.JoinSessionAsync(created.Id, SessionParticipantRole.Controller, created.ControllerJoinToken, CancellationToken.None);

        // Assert
        secondJoin.Status.Should().Be(JoinSessionOperationStatus.ControllerLocked);
        db.DbContext.SessionParticipants.Count(p => p.Role == SessionParticipantRole.Controller).Should().Be(1);
    }

    [Fact]
    public async Task JoinSession_returns_invalid_when_token_is_wrong()
    {
        await using var db = await SqliteTestDb.CreateAsync();
        var service = new SessionLifecycleService(db.DbContext);
        var created = await service.CreateSessionAsync(SessionBuilder.Create().WithName("Midweek").BuildRequest(), CancellationToken.None);

        // Act
        var outcome = await service.JoinSessionAsync(created.Id, SessionParticipantRole.Display, "WRONG", CancellationToken.None);

        // Assert
        outcome.Status.Should().Be(JoinSessionOperationStatus.InvalidToken);
        db.DbContext.SessionParticipants.Should().BeEmpty();
    }
}
