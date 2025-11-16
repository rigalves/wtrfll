using System.Globalization;
using Microsoft.Extensions.Options;
using Wtrfll.Server.Slices.Passages.Application;
using Wtrfll.Server.Slices.Passages.Infrastructure.Providers.Models;
using Wtrfll.Server.Slices.Passages.Infrastructure.Providers.Parsing;

namespace Wtrfll.Server.Slices.Passages.Infrastructure.Providers;

public sealed class NormalizedJsonPassageProvider : IPassageProvider
{
    private readonly IReadOnlyDictionary<string, TranslationEntry> _translations;

    public NormalizedJsonPassageProvider(
        IOptions<NormalizedTranslationOptions> options,
        IHostEnvironment environment,
        ILogger<NormalizedJsonPassageProvider> logger)
    {
        var entries = new Dictionary<string, TranslationEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var translation in options.Value.Translations)
        {
            var path = Path.Combine(environment.ContentRootPath, translation.FileName);
            if (!File.Exists(path))
            {
                logger.LogWarning("Normalized translation file {File} for {Code} not found; skipping", translation.FileName, translation.Code);
                continue;
            }

            entries[translation.Code] = new TranslationEntry(translation, path);
        }

        _translations = entries;
    }

    public bool CanHandle(string translationCode)
        => _translations.ContainsKey(translationCode);

    public async Task<PassageResultDto?> GetPassageAsync(string translationCode, string reference, CancellationToken cancellationToken)
    {
        if (!_translations.TryGetValue(translationCode, out var entry))
        {
            return null;
        }

        var parsed = ScriptureReferenceParser.Parse(reference);
        if (parsed is null)
        {
            return null;
        }

        var translationData = await entry.GetDataAsync();
        var book = translationData.FindBook(parsed.BookToken);
        if (book is null)
        {
            return null;
        }

        if (!book.Chapters.TryGetValue(parsed.Chapter, out var chapterVerses))
        {
            return null;
        }

        IEnumerable<ScriptureVerseRange> ranges = parsed.VerseRanges.Count > 0
            ? parsed.VerseRanges
            : new[] { new ScriptureVerseRange(1, int.MaxValue) };

        var verses = new List<VerseDto>();
        foreach (var range in ranges)
        {
            foreach (var verse in chapterVerses)
            {
                if (verse.Key < range.StartVerse || verse.Key > range.EndVerse)
                {
                    continue;
                }

                verses.Add(new VerseDto
                {
                    Book = book.DisplayName,
                    Chapter = parsed.Chapter,
                    Verse = verse.Key,
                    Text = verse.Value,
                });
            }
        }

        if (verses.Count == 0)
        {
            return null;
        }

        var referenceLabel = BuildReferenceLabel(book.DisplayName, parsed.Chapter, parsed.VerseRanges);

        return new PassageResultDto
        {
            Reference = referenceLabel,
            Translation = translationCode,
            Verses = verses,
            CachePolicy = entry.CachePolicy,
            Attribution = new AttributionDto
            {
                Required = entry.AttributionRequired,
                Text = entry.AttributionText,
                Url = entry.AttributionUrl,
            },
        };
    }

    private static string BuildReferenceLabel(string bookName, int chapter, IReadOnlyList<ScriptureVerseRange> ranges)
    {
        if (ranges.Count == 0)
        {
            return $"{bookName} {chapter}";
        }

        var parts = ranges.Select(range =>
            range.StartVerse == range.EndVerse
                ? range.StartVerse.ToString(CultureInfo.InvariantCulture)
                : $"{range.StartVerse}-{range.EndVerse}");

        return $"{bookName} {chapter}:{string.Join(",", parts)}";
    }
}


