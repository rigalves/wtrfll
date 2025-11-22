using System.Text.RegularExpressions;

namespace Wtrfll.Server.Slices.Lyrics.Infrastructure;

public static class LyricsChordProParser
{
    private static readonly Regex DirectivePattern = new(@"\{[^}]+\}", RegexOptions.Compiled);
    private static readonly Regex ChordPattern = new(@"\[[^\]]+\]", RegexOptions.Compiled);
    private static readonly Regex CommentDirectivePattern = new(@"\{comment:([^}]+)\}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static IReadOnlyList<string> ExtractLines(string chordProText)
    {
        if (string.IsNullOrWhiteSpace(chordProText))
        {
            return Array.Empty<string>();
        }

        var lines = new List<string>();
        foreach (var rawLine in chordProText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
        {
            var commentMatch = CommentDirectivePattern.Match(rawLine);
            var commentText = commentMatch.Success ? commentMatch.Groups[1].Value.Trim() : null;

            var line = DirectivePattern.Replace(rawLine, string.Empty);
            line = ChordPattern.Replace(line, string.Empty);
            line = line.Replace("|", " ").Trim();
            if (line.Length > 0)
            {
                lines.Add(line);
            }
            else if (!string.IsNullOrWhiteSpace(commentText))
            {
                lines.Add($"__COMMENT__ {commentText}");
            }
        }

        return lines;
    }
}
