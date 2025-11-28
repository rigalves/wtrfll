using Wtrfll.Server.Slices.BibleBooks.Application;
using Wtrfll.Server.Slices.BibleBooks.Domain;

namespace Wtrfll.Server.Tests.Common;

public sealed class FakeBibleBookMetadataStore : IBibleBookMetadataStore
{
    private readonly IReadOnlyList<BibleBookMetadata> _books;

    public FakeBibleBookMetadataStore()
    {
        _books = new List<BibleBookMetadata>
        {
            new()
            {
                Id = "GEN",
                EnglishDisplayName = "Genesis",
                SpanishDisplayName = "Génesis",
                Slug = "genesis",
                Ordinal = 1,
                Aliases = new[] { "Genesis", "Gen", "Gn" },
            },
            new()
            {
                Id = "JHN",
                EnglishDisplayName = "John",
                SpanishDisplayName = "Juan",
                Slug = "john",
                Ordinal = 43,
                Aliases = new[] { "John", "Jn", "Juan" },
            },
        };
    }

    public IReadOnlyList<BibleBookMetadata> GetAll() => _books;
}
