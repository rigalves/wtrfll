namespace Wtrfll.Server.Slices.Lyrics.Domain;

public sealed class LyricsEntry
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Author { get; set; }
    public string LyricsChordPro { get; set; } = string.Empty;
    // Optional JSON payload for per-song presentation preferences (font scale, colors, layout, etc.).
    public string? LyricsStyleJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
