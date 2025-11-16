using System.Collections.Immutable;
using System.Text.Json;
using Wtrfll.Translations.Models;
using Wtrfll.Translations.Shared;

namespace Wtrfll.Translations.Parsing;

public sealed class LegacyJsonBibleParser : IBibleParser
{
    private static readonly ImmutableArray<BookMetadata> CanonicalBooks = BookMetadataProvider
        .Load()
        .OrderBy(metadata => metadata.Order)
        .ToImmutableArray();

    public async Task<NormalizedBible> ParseAsync(ParseRequest request)
    {
        await using var stream = File.OpenRead(request.InputPath);
        using var document = await JsonDocument.ParseAsync(stream);

        if (!document.RootElement.TryGetProperty("bible", out var bibleElement))
        {
            throw new InvalidOperationException("Legacy JSON file missing 'bible' root element.");
        }

        var booksElement = bibleElement.GetProperty("b");
        var books = new List<NormalizedBook>();

        foreach (var bookElement in EnumerateArray(booksElement))
        {
            var metadata = ResolveBookMetadata(books.Count);
            var bookId = bookElement.TryGetProperty("_id", out var idProp) ? idProp.GetString() : null;
            var bookTitle = bookElement.TryGetProperty("_name", out var nameProp)
                ? nameProp.GetString()
                : bookElement.TryGetProperty("_t", out var titleProp)
                    ? titleProp.GetString()
                    : bookId;

            var chapters = new List<NormalizedChapter>();
            var chapterElements = bookElement.GetProperty("c");

            foreach (var chapterElement in EnumerateArray(chapterElements))
            {
                var chapterNumber = chapterElement.TryGetProperty("_n", out var chapterNumberProp)
                    ? ParseInt(chapterNumberProp.GetString())
                    : chapters.Count + 1;

                var verses = new List<NormalizedVerse>();
                if (chapterElement.TryGetProperty("v", out var verseElements))
                {
                    foreach (var verseElement in EnumerateArray(verseElements))
                    {
                        var verseNumber = verseElement.TryGetProperty("_n", out var verseNumberProp)
                            ? ParseInt(verseNumberProp.GetString())
                            : verses.Count + 1;
                        var text = verseElement.TryGetProperty("__text", out var textProp)
                            ? textProp.GetString() ?? string.Empty
                            : string.Empty;
                        verses.Add(new NormalizedVerse
                        {
                            Number = verseNumber,
                            Text = text.Trim(),
                        });
                    }
                }

                chapters.Add(new NormalizedChapter
                {
                    Number = chapterNumber,
                    Verses = verses,
                });
            }

            books.Add(new NormalizedBook
            {
                Id = (bookId ?? metadata.Id) ?? $"book_{books.Count + 1}",
                Title = (bookTitle ?? metadata.GetTitle(request.LanguageCode)) ?? $"Book {books.Count + 1}",
                Aliases = metadata.GetAliases(),
                Chapters = chapters,
            });
        }

        return new NormalizedBible
        {
            Code = request.TranslationCode,
            Name = request.TranslationName,
            Language = request.LanguageCode,
            Books = books,
        };
    }

    private static IEnumerable<JsonElement> EnumerateArray(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Array => element.EnumerateArray(),
            JsonValueKind.Object => new[] { element },
            _ => Array.Empty<JsonElement>(),
        };
    }

    private static int ParseInt(string? value)
    {
        if (int.TryParse(value, out var number))
        {
            return number;
        }

        return 0;
    }

    private static BookMetadata ResolveBookMetadata(int index)
    {
        if (index >= 0 && index < CanonicalBooks.Length)
        {
            return CanonicalBooks[index];
        }

        var fallbackId = $"book_{index + 1}";
        var fallbackTitle = $"Book {index + 1}";
        return BookMetadata.CreateFallback(fallbackId, index + 1, fallbackTitle);
    }
}





