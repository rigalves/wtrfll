using Wtrfll.Server.Slices.Lyrics.Application;
using Wtrfll.Server.Slices.Lyrics.Application.Models;

namespace Wtrfll.Server.Slices.Lyrics.Endpoints;

public static class LyricsModule
{
    public static IServiceCollection AddLyricsModule(this IServiceCollection services)
    {
        services.AddScoped<LyricsReadService>();
        services.AddScoped<LyricsPresentationService>();
        return services;
    }

    public static IEndpointRouteBuilder MapLyricsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/lyrics");

        group.MapGet(string.Empty, async (string? search, LyricsReadService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetLyricsAsync(search, cancellationToken)))
            .WithName("ListLyricsEntries")
            .WithOpenApi();

        group.MapGet("/{id:guid}", async (Guid id, LyricsReadService service, CancellationToken cancellationToken) =>
        {
            var entry = await service.GetLyricsAsync(id, cancellationToken);
            return entry is null ? Results.NotFound() : Results.Ok(entry);
        })
        .WithName("GetLyricsEntry")
        .WithOpenApi();

        group.MapPost(string.Empty, async (UpsertLyricsEntryRequest request, LyricsReadService service, CancellationToken cancellationToken) =>
        {
            var saved = await service.SaveLyricsAsync(null, request, cancellationToken);
            return Results.Created($"/api/lyrics/{saved.Id}", saved);
        }).WithName("CreateLyricsEntry").WithOpenApi();

        group.MapPut("/{id:guid}", async (Guid id, UpsertLyricsEntryRequest request, LyricsReadService service, CancellationToken cancellationToken) =>
        {
            var saved = await service.SaveLyricsAsync(id, request, cancellationToken);
            return Results.Ok(saved);
        }).WithName("UpdateLyricsEntry").WithOpenApi();

        return endpoints;
    }
}
