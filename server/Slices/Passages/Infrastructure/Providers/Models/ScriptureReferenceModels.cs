namespace Wtrfll.Server.Slices.Passages.Infrastructure.Providers.Models;

internal sealed class ParsedScriptureReference
{
    public required string BookToken { get; init; }
    public required int Chapter { get; init; }
    public required List<ScriptureVerseRange> VerseRanges { get; init; }
}

internal sealed record ScriptureVerseRange(int StartVerse, int EndVerse);
