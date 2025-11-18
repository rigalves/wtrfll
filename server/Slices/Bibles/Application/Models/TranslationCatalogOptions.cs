using System.ComponentModel.DataAnnotations;

namespace Wtrfll.Server.Slices.Bibles.Application.Models;

public sealed class TranslationCatalogOptions
{
    public const string SectionName = "Bibles:Catalog";

    [Required]
    public List<BibleTranslationOption> Translations { get; init; } = new();
}

public sealed class BibleTranslationOption
{
    [Required]
    public string Code { get; init; } = string.Empty;

    [Required]
    public string Name { get; init; } = string.Empty;

    [Required]
    public string Language { get; init; } = string.Empty;

    [Required]
    public string Provider { get; init; } = string.Empty;

    [Required]
    public string CachePolicy { get; init; } = "no-store";

    public string? License { get; init; }

    public string? Version { get; init; }

    public bool IsOfflineReady { get; init; }
}
