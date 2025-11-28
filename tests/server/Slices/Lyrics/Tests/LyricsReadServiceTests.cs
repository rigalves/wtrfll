using FluentAssertions;
using Wtrfll.Server.Slices.Lyrics.Application;
using Wtrfll.Server.Slices.Lyrics.Application.Models;
using Wtrfll.Server.Tests.Common;
using Xunit;

namespace Wtrfll.Server.Tests.Slices.Lyrics.Tests;

public sealed class LyricsReadServiceTests
{
    [Fact]
    public async Task Save_and_get_entry_roundtrip()
    {
        await using var db = await SqliteTestDb.CreateAsync();
        var service = new LyricsReadService(db.DbContext);
        var request = new UpsertLyricsEntryRequest("Amazing Grace", "Newton", "Amazing grace how sweet the sound");

        var saved = await service.SaveLyricsAsync(null, request, CancellationToken.None);
        saved.Title.Should().Be("Amazing Grace");
        saved.Author.Should().Be("Newton");

        var fetched = await service.GetLyricsAsync(saved.Id, CancellationToken.None);
        fetched.Should().NotBeNull();
        fetched!.Title.Should().Be("Amazing Grace");
        fetched.Author.Should().Be("Newton");
        fetched.LyricsChordPro.Should().Contain("Amazing grace");
    }

    [Fact]
    public async Task Search_matches_title_author_and_lyrics_text()
    {
        await using var db = await SqliteTestDb.CreateAsync();
        var service = new LyricsReadService(db.DbContext);

        await service.SaveLyricsAsync(null, new UpsertLyricsEntryRequest("Digno y Santo", "Tradicional", "Digno y santo"), CancellationToken.None);
        await service.SaveLyricsAsync(null, new UpsertLyricsEntryRequest("Mi Roca", "Living Streams", "mi roca"), CancellationToken.None);
        await service.SaveLyricsAsync(null, new UpsertLyricsEntryRequest("Grace", null, "Amazing grace"), CancellationToken.None);

        var byTitle = await service.GetLyricsAsync("Roca", CancellationToken.None);
        byTitle.Should().ContainSingle(e => e.Title == "Mi Roca");

        var byAuthor = await service.GetLyricsAsync("Tradicional", CancellationToken.None);
        byAuthor.Should().ContainSingle(e => e.Title == "Digno y Santo");

        var byLyrics = await service.GetLyricsAsync("grace", CancellationToken.None);
        byLyrics.Should().ContainSingle(e => e.Title == "Grace");
    }

    [Fact]
    public async Task Update_existing_entry_modifies_fields()
    {
        await using var db = await SqliteTestDb.CreateAsync();
        var service = new LyricsReadService(db.DbContext);
        var created = await service.SaveLyricsAsync(null, new UpsertLyricsEntryRequest("Start", null, "line1"), CancellationToken.None);

        var updated = await service.SaveLyricsAsync(created.Id, new UpsertLyricsEntryRequest("Updated", "Author", "line1\nline2"), CancellationToken.None);

        updated.Title.Should().Be("Updated");
        updated.Author.Should().Be("Author");
        var fetched = await service.GetLyricsAsync(created.Id, CancellationToken.None);
        fetched!.Title.Should().Be("Updated");
        fetched.LyricsChordPro.Should().Contain("line2");
    }

    [Fact]
    public async Task Update_nonexistent_entry_throws()
    {
        await using var db = await SqliteTestDb.CreateAsync();
        var service = new LyricsReadService(db.DbContext);
        var act = () => service.SaveLyricsAsync(Guid.NewGuid(), new UpsertLyricsEntryRequest("Missing", null, "text"), CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
