using System.Globalization;
using System.Text;
using Wtrfll.Server.Slices.Passages.Infrastructure.Providers.Models;

namespace Wtrfll.Server.Slices.Passages.Infrastructure.Providers.Parsing;

internal static class ScriptureReferenceParser
{
    private static readonly char[] Digits = "0123456789".ToCharArray();

    public static ParsedScriptureReference? Parse(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var sanitized = Sanitize(input);
        var firstDigitIndex = sanitized.IndexOfAny(Digits);
        if (firstDigitIndex <= 0)
        {
            return null;
        }

        var bookToken = sanitized[..firstDigitIndex].Trim();
        var numbersPart = sanitized[firstDigitIndex..].Trim();
        if (string.IsNullOrWhiteSpace(bookToken) || string.IsNullOrWhiteSpace(numbersPart))
        {
            return null;
        }

        var parts = numbersPart.Split(':', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var chapter) || chapter <= 0)
        {
            return null;
        }

        var ranges = new List<ScriptureVerseRange>();
        if (parts.Length > 1)
        {
            var segments = parts[1].Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var segment in segments)
            {
                var normalizedSegment = segment.Replace('\u2013', '-').Replace('\u2014', '-');
                var rangeParts = normalizedSegment.Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (!int.TryParse(rangeParts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var start) || start <= 0)
                {
                    return null;
                }

                var end = start;
                if (rangeParts.Length > 1)
                {
                    if (!int.TryParse(rangeParts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out end) || end < start)
                    {
                        return null;
                    }
                }

                ranges.Add(new ScriptureVerseRange(start, end));
            }
        }

        return new ParsedScriptureReference
        {
            BookToken = bookToken,
            Chapter = chapter,
            VerseRanges = ranges,
        };
    }

    public static string? NormalizeToken(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Normalize(NormalizationForm.FormD);
        Span<char> buffer = stackalloc char[normalized.Length];
        var bufferIndex = 0;
        foreach (var ch in normalized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (unicodeCategory == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(ch))
            {
                buffer[bufferIndex++] = char.ToLowerInvariant(ch);
            }
        }

        return bufferIndex == 0 ? null : new string(buffer[..bufferIndex]);
    }

    private static string Sanitize(string value)
        => string.Join(' ', value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
}



