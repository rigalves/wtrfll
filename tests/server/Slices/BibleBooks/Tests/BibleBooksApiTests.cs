using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Wtrfll.Server.Slices.BibleBooks.Domain;
using Wtrfll.Server.Tests.Common;
using Xunit;

namespace Wtrfll.Server.Tests.Slices.BibleBooks.Tests;

public sealed class BibleBooksApiTests : IClassFixture<BibleBooksApiFactory>
{
    private readonly BibleBooksApiFactory _factory;

    public BibleBooksApiTests(BibleBooksApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_bible_books_returns_fake_store_entries()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/bible-books");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var books = await response.Content.ReadFromJsonAsync<List<BibleBookMetadata>>(TestJson.Default);
        books.Should().NotBeNull();
        books!.Should().ContainSingle(b => b.Id == "GEN" && b.EnglishDisplayName == "Genesis");
        books.Should().ContainSingle(b => b.Id == "JHN" && b.SpanishDisplayName == "Juan");
    }
}
