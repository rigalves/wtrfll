using System.Text.Json;
using FluentAssertions;
using Wtrfll.Server.Slices.Sessions.Realtime;
using Wtrfll.Server.Slices.Passages.Application;
using Xunit;

namespace Wtrfll.Server.Tests.CrossCutting;

public sealed class RealtimeContractsSmokeTests
{
    [Fact]
    public void Contract_version_is_nonzero()
    {
        SessionRealtimeContracts.ContractVersion.Should().BeGreaterThan(0);
    }

    [Fact]
    public void State_update_serializes_and_deserializes()
    {
        var message = new SessionStateUpdateMessage
        {
            ContractVersion = SessionRealtimeContracts.ContractVersion,
            SessionId = Guid.NewGuid(),
            State = new SessionRealtimeState
            {
                Translation = "FAKE",
                Reference = "John 1:1",
                Verses = new List<VerseDto> { new() { Book = "John", Chapter = 1, Verse = 1, Text = "In the beginning" } },
                Options = new SessionPresentationOptions { ShowReference = true, ShowVerseNumbers = true },
                CurrentIndex = 0,
                DisplayCommand = SessionDisplayCommands.Normal,
            },
        };

        var json = JsonSerializer.Serialize(message);
        var roundTrip = JsonSerializer.Deserialize<SessionStateUpdateMessage>(json);

        roundTrip.Should().NotBeNull();
        roundTrip!.ContractVersion.Should().Be(message.ContractVersion);
        roundTrip.State.Reference.Should().Be("John 1:1");
        roundTrip.State.Verses.Should().HaveCount(1);
    }

    [Fact]
    public void State_patch_serializes_and_deserializes()
    {
        var message = new SessionStatePatchMessage
        {
            ContractVersion = SessionRealtimeContracts.ContractVersion,
            SessionId = Guid.NewGuid(),
            Patch = new SessionStatePatchBody
            {
                Translation = "FAKE",
                PassageRef = "John 1:1",
                CurrentIndex = 0,
                Options = new SessionPresentationOptions { ShowReference = true, ShowVerseNumbers = false },
                DisplayCommand = SessionDisplayCommands.Clear,
            },
        };

        var json = JsonSerializer.Serialize(message);
        var roundTrip = JsonSerializer.Deserialize<SessionStatePatchMessage>(json);

        roundTrip.Should().NotBeNull();
        roundTrip!.Patch!.Translation.Should().Be("FAKE");
        roundTrip.Patch.PassageRef.Should().Be("John 1:1");
    }
}
