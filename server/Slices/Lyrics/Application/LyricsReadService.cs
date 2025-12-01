using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Wtrfll.Server.Infrastructure.Data;
using Wtrfll.Server.Slices.Lyrics.Application.Models;
using Wtrfll.Server.Slices.Lyrics.Domain;

namespace Wtrfll.Server.Slices.Lyrics.Application;

public sealed class LyricsReadService
{
    private readonly AppDbContext dbContext;

    public LyricsReadService(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IReadOnlyList<LyricsEntrySummaryDto>> GetLyricsAsync(string? query, CancellationToken cancellationToken)
    {
        var entries = dbContext.LyricsEntries.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query))
        {
            var like = $"%{query.Trim()}%";
            entries = entries.Where(entry =>
                EF.Functions.Like(entry.Title, like) ||
                (entry.Author != null && EF.Functions.Like(entry.Author, like)) ||
                EF.Functions.Like(entry.LyricsChordPro, like));
        }

        return await entries
            .OrderBy(entry => entry.Title)
            .Select(entry => new LyricsEntrySummaryDto(entry.Id, entry.Title, entry.Author))
            .ToListAsync(cancellationToken);
    }

    public async Task<LyricsEntryDetailDto?> GetLyricsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.LyricsEntries
            .AsNoTracking()
            .Where(entry => entry.Id == id)
            .Select(entry => new LyricsEntryDetailDto(
                entry.Id,
                entry.Title,
                entry.Author,
                entry.LyricsChordPro,
                DeserializeStyle(entry.LyricsStyleJson)))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<LyricsEntryDetailDto> SaveLyricsAsync(Guid? id, UpsertLyricsEntryRequest request, CancellationToken cancellationToken)
    {
        LyricsEntry entry;
        if (id.HasValue)
        {
            entry = await dbContext.LyricsEntries.FirstOrDefaultAsync(e => e.Id == id.Value, cancellationToken)
                ?? throw new KeyNotFoundException("Lyrics entry not found.");
            entry.Title = request.Title;
            entry.Author = request.Author;
            entry.LyricsChordPro = request.LyricsChordPro;
            entry.LyricsStyleJson = SerializeStyle(request.Style);
            entry.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            entry = new LyricsEntry
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Author = request.Author,
                LyricsChordPro = request.LyricsChordPro,
                LyricsStyleJson = SerializeStyle(request.Style),
                CreatedAt = DateTime.UtcNow,
            };
            await dbContext.LyricsEntries.AddAsync(entry, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return new LyricsEntryDetailDto(entry.Id, entry.Title, entry.Author, entry.LyricsChordPro, DeserializeStyle(entry.LyricsStyleJson));
    }

    private static string? SerializeStyle(LyricsStyleDto? style)
    {
        if (style is null)
        {
            return null;
        }

        var normalized = NormalizeStyle(style);
        return JsonSerializer.Serialize(normalized);
    }

    private static LyricsStyleDto? DeserializeStyle(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            var dto = JsonSerializer.Deserialize<LyricsStyleDto>(json);
            return dto is null ? null : NormalizeStyle(dto);
        }
        catch
        {
            return null;
        }
    }

    private static LyricsStyleDto NormalizeStyle(LyricsStyleDto style)
    {
        double? clamped = style.FontScale;
        if (clamped.HasValue)
        {
            clamped = Math.Clamp(clamped.Value, 0.6, 3.0);
        }
        return new LyricsStyleDto(clamped);
    }
}
