namespace Wtrfll.Translations.Models;

internal sealed class BookMetadata
{
    public BookMetadata(
        string id,
        int order,
        IDictionary<string, string> titles,
        IDictionary<string, string[]> aliases)
    {
        Id = id;
        Order = order;
        Titles = titles;
        Aliases = aliases;
    }

    public string Id { get; }
    public int Order { get; }
    public IDictionary<string, string> Titles { get; }
    public IDictionary<string, string[]> Aliases { get; }

    public string GetTitle(string? languageCode)
    {
        if (!string.IsNullOrWhiteSpace(languageCode))
        {
            if (Titles.TryGetValue(languageCode, out var title))
            {
                return title;
            }

            var partialKey = Titles.Keys.FirstOrDefault(key => languageCode.StartsWith(key, StringComparison.OrdinalIgnoreCase));
            if (partialKey is not null && Titles.TryGetValue(partialKey, out var partialTitle))
            {
                return partialTitle;
            }
        }

        if (Titles.TryGetValue("en", out var englishTitle))
        {
            return englishTitle;
        }

        return Titles.Values.FirstOrDefault() ?? Id;
    }

    public IReadOnlyList<string> GetAliases()
    {
        var aliasSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var aliasGroup in Aliases.Values)
        {
            foreach (var alias in aliasGroup)
            {
                aliasSet.Add(alias);
            }
        }

        foreach (var title in Titles.Values)
        {
            aliasSet.Add(title);

            var shortAlias = GenerateShortAlias(title);
            if (!string.IsNullOrWhiteSpace(shortAlias))
            {
                aliasSet.Add(shortAlias);
            }
        }

        aliasSet.Add(GenerateShortAlias(Id));

        return aliasSet.ToList();
    }

    public static BookMetadata CreateFallback(string id, int order, string defaultTitle)
    {
        var titles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = defaultTitle
        };

        var aliases = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = new[] { defaultTitle }
        };

        return new BookMetadata(id, order, titles, aliases);
    }

    private static string GenerateShortAlias(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = new string(value.Where(char.IsLetter).Take(2).ToArray());
        return trimmed.ToLowerInvariant();
    }
}
