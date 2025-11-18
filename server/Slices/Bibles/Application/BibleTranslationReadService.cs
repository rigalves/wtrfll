using Wtrfll.Server.Slices.Bibles.Domain;

namespace Wtrfll.Server.Slices.Bibles.Application;

public sealed class BibleTranslationReadService
{
    private readonly IBibleTranslationCatalog _catalog;

    public BibleTranslationReadService(IBibleTranslationCatalog catalog)
    {
        _catalog = catalog;
    }

    public IReadOnlyList<BibleTranslationDto> GetTranslations()
    {
        return _catalog
            .GetAll()
            .Select(MapToDto)
            .ToList();
    }

    private static BibleTranslationDto MapToDto(BibleTranslation translation)
    {
        return new BibleTranslationDto
        {
            Code = translation.Code,
            Name = translation.Name,
            Language = translation.Language,
            Provider = translation.Provider,
            CachePolicy = translation.CachePolicy,
            License = translation.License,
            Version = translation.Version,
            IsOfflineReady = translation.IsOfflineReady,
        };
    }
}



