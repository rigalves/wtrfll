using FluentAssertions;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Wtrfll.Server.Slices.Sessions.Application;
using Wtrfll.Server.Slices.Sessions.Domain;
using Wtrfll.Server.Slices.Sessions.Realtime;
using Wtrfll.Server.Tests.Common;
using Xunit;

namespace Wtrfll.Server.Tests.Slices.Sessions.Tests;

[Collection("SessionRealtime")]

public sealed class SessionHubTests : IClassFixture<SessionApiFactory>
{
    private readonly SessionApiFactory _factory;

    public SessionHubTests(SessionApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Controller_patch_broadcasts_to_display_group()
    {
        await _factory.ResetDatabaseAsync();

        // Arrange: create session and tokens
        var session = await CreateSessionAsync(SessionBuilder.Create().WithName("Live").BuildRequest());
        var controller = CreateHubConnection(session.Id, SessionParticipantRole.Controller, session.ControllerJoinToken);
        var display = CreateHubConnection(session.Id, SessionParticipantRole.Display, session.DisplayJoinToken);

        var received = new TaskCompletionSource<SessionStateUpdateMessage>();
        display.On<SessionStateUpdateMessage>("state:update", update =>
        {
            received.TrySetResult(update);
        });

        await controller.StartAsync();
        await display.StartAsync();
        await Task.Delay(100);

        // Act: controller sends a state patch
        await controller.InvokeAsync("StatePatch", new SessionStatePatchMessage
        {
            ContractVersion = SessionRealtimeContracts.ContractVersion,
            SessionId = session.Id,
            Patch = new SessionStatePatchBody
            {
                Translation = "FAKE",
                PassageRef = "John 3:16",
                CurrentIndex = 0,
                DisplayCommand = SessionDisplayCommands.Black,
            },
        });

        var update = await received.Task.WaitAsync(TimeSpan.FromSeconds(10));

        // Assert
        update.SessionId.Should().Be(session.Id);
        update.State.Translation.Should().Be("FAKE");
        update.State.Reference.Should().Be("John 3:16");
        update.State.Verses.Should().ContainSingle(v => v.Verse == 16 && v.Book == "John");
        update.State.DisplayCommand.Should().Be(SessionDisplayCommands.Black);

        await controller.DisposeAsync();
        await display.DisposeAsync();
    }

    [Fact]
    public async Task Display_cannot_send_state_patch()
    {
        await _factory.ResetDatabaseAsync();
        var session = await CreateSessionAsync(SessionBuilder.Create().WithName("Live").BuildRequest());
        var display = CreateHubConnection(session.Id, SessionParticipantRole.Display, session.DisplayJoinToken);

        await display.StartAsync();
        await Task.Delay(100);

        var act = () => display.InvokeAsync("StatePatch", new SessionStatePatchMessage
        {
            ContractVersion = SessionRealtimeContracts.ContractVersion,
            SessionId = session.Id,
            Patch = new SessionStatePatchBody
            {
                Translation = "FAKE",
                PassageRef = "John 1:1",
                CurrentIndex = 0,
            },
        });

        await act.Should().ThrowAsync<HubException>().WithMessage("*controller*");

        await display.DisposeAsync();
    }

    [Fact]
    public async Task StatePatch_rejects_mismatched_contract_version()
    {
        await _factory.ResetDatabaseAsync();
        var session = await CreateSessionAsync(SessionBuilder.Create().WithName("Live").BuildRequest());
        var controller = CreateHubConnection(session.Id, SessionParticipantRole.Controller, session.ControllerJoinToken);

        await controller.StartAsync();
        await Task.Delay(100);

        var act = () => controller.InvokeAsync("StatePatch", new SessionStatePatchMessage
        {
            ContractVersion = SessionRealtimeContracts.ContractVersion + 99,
            SessionId = session.Id,
            Patch = new SessionStatePatchBody
            {
                Translation = "FAKE",
                PassageRef = "John 3:16",
                CurrentIndex = 0,
            },
        });

        await act.Should().ThrowAsync<HubException>().WithMessage("*contract version*");

        await controller.DisposeAsync();
    }

    private async Task<SessionCreatedDto> CreateSessionAsync(CreateSessionRequestDto request)
    {
        using var scope = _factory.Services.CreateScope();
        var lifecycle = scope.ServiceProvider.GetRequiredService<SessionLifecycleService>();
        return await lifecycle.CreateSessionAsync(request, CancellationToken.None);
    }

    private HubConnection CreateHubConnection(Guid sessionId, SessionParticipantRole role, string joinToken)
    {
        var baseUri = _factory.Server.BaseAddress ?? new Uri("http://localhost");
        var url = new UriBuilder(baseUri)
        {
            Path = "/realtime",
            Query = $"sessionId={sessionId}&role={role}&joinToken={joinToken}",
        }.Uri;

        return new HubConnectionBuilder()
            .WithUrl(url, options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.Transports = HttpTransportType.LongPolling;
            })
            .WithAutomaticReconnect()
            .Build();
    }
}
