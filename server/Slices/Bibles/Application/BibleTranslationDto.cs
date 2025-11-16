namespace Wtrfll.Server.Slices.Bibles.Application;

public sealed record BibleTranslationDto
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string Language { get; init; }
    public required string Provider { get; init; }
    public required string CachePolicy { get; init; }
    public string? License { get; init; }
    public string? Version { get; init; }
}



