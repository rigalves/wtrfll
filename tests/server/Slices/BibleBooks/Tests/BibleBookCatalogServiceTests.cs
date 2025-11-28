using FluentAssertions;
using Wtrfll.Server.Slices.BibleBooks.Application;
using Wtrfll.Server.Tests.Common;
using Xunit;

namespace Wtrfll.Server.Tests.Slices.BibleBooks.Tests;

public sealed class BibleBookCatalogServiceTests
{
    [Fact]
    public void Returns_books_from_store_in_order()
    {
        var store = new FakeBibleBookMetadataStore();
        var service = new BibleBookCatalogService(store);

        var result = service.GetAll();

        result.Should().NotBeNull();
        result.Count.Should().BeGreaterThan(0);
        result.First().Ordinal.Should().NotBeNull();
        result.Last().Ordinal.Should().NotBeNull();
        result.First().Ordinal!.Value.Should().BeLessOrEqualTo(result.Last().Ordinal!.Value);
    }
}
