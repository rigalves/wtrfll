namespace Wtrfll.Server.Slices.Lyrics.Application.Models;

public sealed record LyricsEntrySummaryDto(Guid Id, string Title, string? Author);

public sealed record LyricsStyleDto(double? FontScale, int? ColumnCount);

public sealed record LyricsEntryDetailDto(
    Guid Id,
    string Title,
    string? Author,
    string LyricsChordPro,
    LyricsStyleDto? Style);

public sealed record UpsertLyricsEntryRequest(string Title, string? Author, string LyricsChordPro, LyricsStyleDto? Style);
