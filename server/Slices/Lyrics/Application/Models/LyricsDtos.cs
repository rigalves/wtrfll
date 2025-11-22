namespace Wtrfll.Server.Slices.Lyrics.Application.Models;

public sealed record LyricsEntrySummaryDto(Guid Id, string Title, string? Author);

public sealed record LyricsEntryDetailDto(Guid Id, string Title, string? Author, string LyricsChordPro);

public sealed record UpsertLyricsEntryRequest(string Title, string? Author, string LyricsChordPro);
