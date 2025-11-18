using Microsoft.Extensions.Options;
using Wtrfll.Server.Slices.Bibles.Domain;
using Wtrfll.Server.Slices.Bibles.Application.Models;

namespace Wtrfll.Server.Slices.Bibles.Application;

public sealed class BibleTranslationCatalog : IBibleTranslationCatalog
{
    private readonly IReadOnlyList<BibleTranslation> _translations;

    public BibleTranslationCatalog(IOptions<TranslationCatalogOptions> options)
    {
        _translations = options.Value.Translations
            .Select(option => new BibleTranslation(
                option.Code,
                option.Name,
                option.Language,
                option.Provider,
                option.CachePolicy,
                option.License,
                option.Version,
                option.IsOfflineReady))
            .ToList();
    }

    public IReadOnlyList<BibleTranslation> GetAll() => _translations;
}



