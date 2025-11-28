using FluentAssertions;
using Wtrfll.Server.Slices.Passages.Application;
using Wtrfll.Server.Tests.Common;
using Xunit;

namespace Wtrfll.Server.Tests.Slices.Passages.Tests;

public sealed class PassageReadServiceTests
{
    [Fact]
    public async Task Returns_null_when_no_provider_handles_translation()
    {
        var service = new PassageReadService(Array.Empty<IPassageProvider>());

        var result = await service.GetPassageAsync("UNKNOWN", "John 1:1", CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Returns_passage_when_provider_handles_translation()
    {
        var provider = new FakePassageProvider("FAKE", "John 1:1");
        var service = new PassageReadService(new[] { provider });

        var result = await service.GetPassageAsync("FAKE", "John 1:1", CancellationToken.None);

        result.Should().NotBeNull();
        result!.Translation.Should().Be("FAKE");
        result.Reference.Should().Be("John 1:1");
        result.Verses.Should().ContainSingle(v => v.Verse == 16 && v.Book == "John");
    }
}
