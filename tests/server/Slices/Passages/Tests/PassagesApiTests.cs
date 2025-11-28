using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Wtrfll.Server.Slices.Passages.Application;
using Wtrfll.Server.Tests.Common;
using Xunit;

namespace Wtrfll.Server.Tests.Slices.Passages.Tests;

public sealed class PassagesApiTests : IClassFixture<PassagesApiFactory>
{
    private readonly PassagesApiFactory _factory;

    public PassagesApiTests(PassagesApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_passage_returns_not_found_when_unknown_translation()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/passage?translation=UNKNOWN&ref=John+1%3A1");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_passage_returns_result_from_provider()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/passage?translation=FAKE&ref=John+3%3A16");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<PassageResultDto>(TestJson.Default);
        body.Should().NotBeNull();
        body!.Reference.Should().Be("John 3:16");
        body.Translation.Should().Be("FAKE");
        body.Verses.Should().ContainSingle(v => v.Verse == 16 && v.Book == "John");
    }
}
