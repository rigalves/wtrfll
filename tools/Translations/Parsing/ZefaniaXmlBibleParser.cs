using System.Xml.Linq;
using Wtrfll.Translations.Shared;

namespace Wtrfll.Translations.Parsing;

public sealed class ZefaniaXmlBibleParser : IBibleParser
{
    public Task<NormalizedBible> ParseAsync(ParseRequest request)
    {
        var document = XDocument.Load(request.InputPath);
        var xmlBible = document.Root ?? throw new InvalidOperationException("Invalid Zefania XML: missing root element.");

        var books = new List<NormalizedBook>();
        foreach (var bookElement in xmlBible.Elements().Where(e => e.Name.LocalName.Equals("BIBLEBOOK", StringComparison.OrdinalIgnoreCase)))
        {
            var bookId = (string?)bookElement.Attribute("bname")
                        ?? (string?)bookElement.Attribute("bsname")
                        ?? (string?)bookElement.Attribute("id")
                        ?? $"Book_{books.Count + 1}";
            var bookTitle = (string?)bookElement.Attribute("bname") ?? bookId;

            var chapters = new List<NormalizedChapter>();
            foreach (var chapterElement in bookElement.Elements().Where(e => e.Name.LocalName.Equals("CHAPTER", StringComparison.OrdinalIgnoreCase)))
            {
                var chapterNumber = ParseInt((string?)chapterElement.Attribute("cnumber")) ?? chapters.Count + 1;
                var verses = new List<NormalizedVerse>();
                foreach (var verseElement in chapterElement.Elements().Where(e => e.Name.LocalName.Equals("VERS", StringComparison.OrdinalIgnoreCase)))
                {
                    var verseNumber = ParseInt((string?)verseElement.Attribute("vnumber")) ?? verses.Count + 1;
                    var text = verseElement.Value.Trim();
                    verses.Add(new NormalizedVerse
                    {
                        Number = verseNumber,
                        Text = text,
                    });
                }

                chapters.Add(new NormalizedChapter
                {
                    Number = chapterNumber,
                    Verses = verses,
                });
            }

            books.Add(new NormalizedBook
            {
                Id = bookId,
                Title = bookTitle,
                Aliases = new[] { bookTitle },
                Chapters = chapters,
            });
        }

        var normalized = new NormalizedBible
        {
            Code = request.TranslationCode,
            Name = request.TranslationName,
            Language = request.LanguageCode,
            Books = books,
        };

        return Task.FromResult(normalized);
    }

    private static int? ParseInt(string? value)
    {
        if (int.TryParse(value, out var number))
        {
            return number;
        }

        return null;
    }
}
