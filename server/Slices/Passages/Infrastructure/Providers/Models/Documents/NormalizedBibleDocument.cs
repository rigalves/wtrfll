using System.Text.Json.Serialization;

namespace Wtrfll.Server.Slices.Passages.Infrastructure.Providers.Models.Documents;

internal sealed class NormalizedBibleDocument
{
    [JsonPropertyName("code")]
    public string Code { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("language")]
    public string Language { get; init; } = string.Empty;

    [JsonPropertyName("books")]
    public List<NormalizedBookDocument> Books { get; init; } = new();
}

internal sealed class NormalizedBookDocument
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("aliases")]
    public List<string>? Aliases { get; init; }

    [JsonPropertyName("chapters")]
    public List<NormalizedChapterDocument>? Chapters { get; init; }
}

internal sealed class NormalizedChapterDocument
{
    [JsonPropertyName("number")]
    public int Number { get; init; }

    [JsonPropertyName("verses")]
    public List<NormalizedVerseDocument>? Verses { get; init; }
}

internal sealed class NormalizedVerseDocument
{
    [JsonPropertyName("number")]
    public int Number { get; init; }

    [JsonPropertyName("text")]
    public string? Text { get; init; }
}
