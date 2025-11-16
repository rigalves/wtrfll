using Wtrfll.Server.Slices.Passages.Infrastructure.Providers.Models.Documents;
using Wtrfll.Server.Slices.Passages.Infrastructure.Providers.Parsing;

namespace Wtrfll.Server.Slices.Passages.Infrastructure.Providers.Models;

internal sealed class NormalizedTranslationData
{
    private readonly IReadOnlyDictionary<string, NormalizedBook> _bookLookup;

    private NormalizedTranslationData(IReadOnlyDictionary<string, NormalizedBook> bookLookup)
    {
        _bookLookup = bookLookup;
    }

    public static NormalizedTranslationData Create(NormalizedBibleDocument document)
    {
        var lookup = new Dictionary<string, NormalizedBook>(StringComparer.OrdinalIgnoreCase);

        foreach (var book in document.Books)
        {
            var aliasSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                book.Title,
                book.Id,
            };

            if (book.Aliases != null)
            {
                foreach (var alias in book.Aliases)
                {
                    aliasSet.Add(alias);
                }
            }

            var normalizedBook = new NormalizedBook(
                book.Title,
                BuildChapterLookup(book.Chapters));

            foreach (var alias in aliasSet)
            {
                var normalized = ScriptureReferenceParser.NormalizeToken(alias);
                if (string.IsNullOrWhiteSpace(normalized))
                {
                    continue;
                }

                lookup[normalized] = normalizedBook;
            }
        }

        return new NormalizedTranslationData(lookup);
    }

    public NormalizedBook? FindBook(string rawToken)
    {
        var normalized = ScriptureReferenceParser.NormalizeToken(rawToken);
        if (normalized is null)
        {
            return null;
        }

        return _bookLookup.TryGetValue(normalized, out var book) ? book : null;
    }

    private static Dictionary<int, SortedDictionary<int, string>> BuildChapterLookup(IEnumerable<NormalizedChapterDocument>? chapters)
    {
        var chapterLookup = new Dictionary<int, SortedDictionary<int, string>>();
        if (chapters is null)
        {
            return chapterLookup;
        }

        foreach (var chapter in chapters)
        {
            var verseLookup = new SortedDictionary<int, string>();
            if (chapter.Verses != null)
            {
                foreach (var verse in chapter.Verses)
                {
                    verseLookup[verse.Number] = verse.Text?.Trim() ?? string.Empty;
                }
            }

            chapterLookup[chapter.Number] = verseLookup;
        }

        return chapterLookup;
    }
}

internal sealed record NormalizedBook(string DisplayName, Dictionary<int, SortedDictionary<int, string>> Chapters);

