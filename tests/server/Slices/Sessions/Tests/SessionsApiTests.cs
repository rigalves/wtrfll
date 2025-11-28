using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wtrfll.Server.Slices.Sessions.Application;
using Wtrfll.Server.Slices.Sessions.Domain;
using Wtrfll.Server.Tests.Common;
using Xunit;

namespace Wtrfll.Server.Tests.Slices.Sessions;

public sealed class SessionsApiTests : IClassFixture<SessionApiFactory>
{
    private readonly SessionApiFactory _factory;

    public SessionsApiTests(SessionApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_and_join_session_happy_path()
    {
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClient();

        // Act: create
        var createResponse = await client.PostAsJsonAsync("/api/sessions", new CreateSessionRequestDto { Name = "Sunday" });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<SessionCreatedDto>(TestJson.Default);
        created.Should().NotBeNull();

        // Act: join
        var joinResponse = await client.PostAsJsonAsync($"/api/sessions/{created!.Id}/join", new JoinSessionRequestDto
        {
            JoinToken = created.ControllerJoinToken,
            Role = SessionParticipantRole.Controller,
        });

        // Assert
        joinResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var joinResult = await joinResponse.Content.ReadFromJsonAsync<JoinSessionResultDto>(TestJson.Default);
        joinResult!.Ok.Should().BeTrue();
        joinResult.ControllerLocked.Should().BeFalse();
        joinResult.Role.Should().Be(SessionParticipantRole.Controller);
    }

    [Fact]
    public async Task Join_session_with_wrong_token_returns_bad_request()
    {
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClient();
        var created = await (await client.PostAsJsonAsync("/api/sessions", new CreateSessionRequestDto { Name = "Team" }))
            .Content.ReadFromJsonAsync<SessionCreatedDto>(TestJson.Default);

        // Act
        var response = await client.PostAsJsonAsync($"/api/sessions/{created!.Id}/join", new JoinSessionRequestDto
        {
            JoinToken = "WRONG",
            Role = SessionParticipantRole.Display,
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Upcoming_sessions_returns_recent_only()
    {
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClient();

        // Create two recent sessions and one very old that should be filtered out.
        var first = await (await client.PostAsJsonAsync("/api/sessions", new CreateSessionRequestDto { Name = "Today" }))
            .Content.ReadFromJsonAsync<SessionCreatedDto>(TestJson.Default);
        var second = await (await client.PostAsJsonAsync("/api/sessions", new CreateSessionRequestDto { Name = "Tomorrow", ScheduledAt = DateTime.UtcNow.AddDays(1) }))
            .Content.ReadFromJsonAsync<SessionCreatedDto>(TestJson.Default);

        // Manually insert an old session.
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<Wtrfll.Server.Infrastructure.Data.AppDbContext>();
            db.Sessions.Add(new Session
            {
                Id = Guid.NewGuid(),
                ShortCode = "OLD001",
                ControllerJoinCode = "C-OLD",
                DisplayJoinCode = "D-OLD",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                Status = SessionStatus.Pending,
                Name = "Old",
            });
            db.SaveChanges();
        }

        // Act
        var listResponse = await client.GetAsync("/api/sessions/upcoming");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var sessions = await listResponse.Content.ReadFromJsonAsync<List<UpcomingSessionDto>>(TestJson.Default);

        // Assert
        sessions.Should().NotBeNull();
        sessions!.Select(s => s.Id).Should().Contain(new[] { first!.Id, second!.Id });
        sessions!.Any(s => s.ShortCode == "OLD001").Should().BeFalse();
    }

}
