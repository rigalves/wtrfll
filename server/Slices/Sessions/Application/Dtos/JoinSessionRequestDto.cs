using System.Text.Json.Serialization;
using Wtrfll.Server.Slices.Sessions.Domain;

namespace Wtrfll.Server.Slices.Sessions.Application;

public sealed record JoinSessionRequestDto
{
    [JsonPropertyName("role")]
    public SessionParticipantRole Role { get; init; }

    [JsonPropertyName("joinToken")]
    public required string JoinToken { get; init; }
}


