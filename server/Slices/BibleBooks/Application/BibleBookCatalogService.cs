using Wtrfll.Server.Slices.BibleBooks.Domain;

namespace Wtrfll.Server.Slices.BibleBooks.Application;

public interface IBibleBookMetadataStore
{
    IReadOnlyList<BibleBookMetadata> GetAll();
}

public sealed class BibleBookCatalogService
{
    private readonly IBibleBookMetadataStore _store;

    public BibleBookCatalogService(IBibleBookMetadataStore store)
    {
        _store = store;
    }

    public IReadOnlyList<BibleBookMetadata> GetAll()
    {
        return _store.GetAll();
    }
}
