namespace Wtrfll.Server.Slices.Bibles.Domain;

public sealed record BibleTranslation(
    string Code,
    string Name,
    string Language,
    string Provider,
    string CachePolicy,
    string? License,
    string? Version);

