using Wtrfll.Server.Slices.Passages.Application;
using Wtrfll.Server.Slices.Passages.Infrastructure.Providers;
using Wtrfll.Server.Slices.Passages.Infrastructure.Providers.Models;

namespace Wtrfll.Server.Slices.Passages.Endpoints;

public static class PassagesModule
{
    public static IServiceCollection AddPassagesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<NormalizedTranslationOptions>(configuration.GetSection(NormalizedTranslationOptions.SectionName));
        services.AddScoped<PassageReadService>();
        services.AddSingleton<IPassageProvider, NormalizedJsonPassageProvider>();

        return services;
    }

    public static IEndpointRouteBuilder MapPassageEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/passage", async (string translation, string @ref, PassageReadService service, CancellationToken cancellationToken) =>
            {
                var result = await service.GetPassageAsync(translation, @ref, cancellationToken);
                return result is null ? Results.NotFound() : Results.Ok(result);
            })
            .WithName("GetPassage")
            .WithOpenApi();

        return endpoints;
    }
}



