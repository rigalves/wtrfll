namespace Wtrfll.Translations.Shared;

public sealed record NormalizedBible
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string Language { get; init; }
    public required IReadOnlyList<NormalizedBook> Books { get; init; }
}

public sealed record NormalizedBook
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public IReadOnlyList<string> Aliases { get; init; } = Array.Empty<string>();
    public required IReadOnlyList<NormalizedChapter> Chapters { get; init; }
}

public sealed record NormalizedChapter
{
    public required int Number { get; init; }
    public required IReadOnlyList<NormalizedVerse> Verses { get; init; }
}

public sealed record NormalizedVerse
{
    public required int Number { get; init; }
    public required string Text { get; init; }
}
