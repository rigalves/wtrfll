using Wtrfll.Server.Slices.Lyrics.Infrastructure;
using Wtrfll.Server.Slices.Sessions.Realtime;

namespace Wtrfll.Server.Slices.Lyrics.Application;

public sealed class LyricsPresentationService
{
    private readonly LyricsReadService _lyricsReadService;

    public LyricsPresentationService(LyricsReadService lyricsReadService)
    {
        _lyricsReadService = lyricsReadService;
    }

    public async Task<LyricsStatePayload> BuildLyricsStateAsync(LyricsStatePatchBody patch, CancellationToken cancellationToken)
    {
        var chordPro = patch.LyricsChordPro;
        var title = patch.Title;
        var author = patch.Author;

        if (string.IsNullOrWhiteSpace(chordPro) && patch.LyricsId is Guid id)
        {
            var entry = await _lyricsReadService.GetLyricsAsync(id, cancellationToken)
                ?? throw new InvalidOperationException("Lyrics entry not found.");
            chordPro = entry.LyricsChordPro;
            title ??= entry.Title;
            author ??= entry.Author;
        }

        if (string.IsNullOrWhiteSpace(chordPro))
        {
            throw new InvalidOperationException("Lyrics text is required.");
        }

        var lines = LyricsChordProParser.ExtractLines(chordPro);
        var fontScale = patch.FontScale.HasValue && patch.FontScale > 0 ? patch.FontScale : 1.0;

        return new LyricsStatePayload
        {
            LyricsId = patch.LyricsId,
            Title = title,
            Author = author,
            Lines = lines,
            FontScale = fontScale,
        };
    }
}
