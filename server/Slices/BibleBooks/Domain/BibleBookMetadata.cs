namespace Wtrfll.Server.Slices.BibleBooks.Domain;

public sealed record BibleBookMetadata
{
    public required string Id { get; init; }
    public required string EnglishDisplayName { get; init; }
    public required string SpanishDisplayName { get; init; }
    public required string Slug { get; init; }
    public int? Ordinal { get; init; }
    public required IReadOnlyList<string> Aliases { get; init; } = Array.Empty<string>();
}
