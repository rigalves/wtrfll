using System.ComponentModel.DataAnnotations;

namespace Wtrfll.Server.Slices.Passages.Infrastructure.Providers.Models;

public sealed class NormalizedTranslationOptions
{
    public const string SectionName = "Bibles:NormalizedSources";

    [Required]
    public List<NormalizedTranslationSource> Translations { get; init; } = new();
}

public sealed class NormalizedTranslationSource
{
    [Required]
    public string Code { get; init; } = string.Empty;

    [Required]
    public string FileName { get; init; } = string.Empty;

    public string CachePolicy { get; init; } = "no-store";

    public bool AttributionRequired { get; init; }

    public string? AttributionText { get; init; }

    public string? AttributionUrl { get; init; }
}
