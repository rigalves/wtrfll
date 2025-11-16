namespace Wtrfll.Server.Slices.Passages.Application;

public sealed record VerseDto
{
    public required string Book { get; init; }
    public required int Chapter { get; init; }
    public required int Verse { get; init; }
    public required string Text { get; init; }
}

public sealed record AttributionDto
{
    public required bool Required { get; init; }
    public string? Text { get; init; }
    public string? Url { get; init; }
}

public sealed record PassageResultDto
{
    public required string Reference { get; init; }
    public required string Translation { get; init; }
    public required IReadOnlyList<VerseDto> Verses { get; init; }
    public AttributionDto? Attribution { get; init; }
    public string CachePolicy { get; init; } = "no-store";
}


