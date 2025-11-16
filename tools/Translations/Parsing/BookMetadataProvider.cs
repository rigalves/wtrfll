using System.Collections.Immutable;
using System.Text.Json;
using Wtrfll.Translations.Models;

namespace Wtrfll.Translations.Parsing;

internal static class BookMetadataProvider
{
    private const string MetadataFileName = "config/bookMetadata.json";

    public static ImmutableArray<BookMetadata> Load()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var metadataPath = Path.Combine(baseDirectory, MetadataFileName);

        if (!File.Exists(metadataPath))
        {
            throw new FileNotFoundException($"Book metadata file not found at {metadataPath}", metadataPath);
        }

        using var stream = File.OpenRead(metadataPath);
        var document = JsonDocument.Parse(stream);

        if (!document.RootElement.TryGetProperty("books", out var booksElement))
        {
            throw new InvalidOperationException("Invalid book metadata file: missing 'books' element.");
        }

        var builder = ImmutableArray.CreateBuilder<BookMetadata>();

        foreach (var bookJson in booksElement.EnumerateArray())
        {
            var entry = ParseBookMetadata(bookJson);
            builder.Add(entry);
        }

        return builder.ToImmutable();
    }

    private static BookMetadata ParseBookMetadata(JsonElement element)
    {
        var id = element.GetProperty("id").GetString() ?? string.Empty;
        var order = element.GetProperty("order").GetInt32();
        var titles = element.GetProperty("titles")
            .EnumerateObject()
            .ToDictionary(
                prop => prop.Name,
                prop => prop.Value.GetString() ?? prop.Name,
                StringComparer.OrdinalIgnoreCase);

        var aliases = element.GetProperty("aliases")
            .EnumerateObject()
            .ToDictionary(
                prop => prop.Name,
                prop => prop.Value.EnumerateArray()
                    .Select(value => value.GetString() ?? string.Empty)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray(),
                StringComparer.OrdinalIgnoreCase);

        return new BookMetadata(id, order, titles, aliases);
    }
}
