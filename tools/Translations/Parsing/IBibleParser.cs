namespace Wtrfll.Translations.Parsing;

using Wtrfll.Translations.Shared;

public interface IBibleParser
{
    Task<NormalizedBible> ParseAsync(ParseRequest request);
}
