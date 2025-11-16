using Wtrfll.Server.Slices.Bibles.Application;
using Wtrfll.Server.Slices.Bibles.Application.Models;

namespace Wtrfll.Server.Slices.Bibles.Endpoints;

public static class BiblesModule
{
    public static IServiceCollection AddBiblesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TranslationCatalogOptions>(configuration.GetSection(TranslationCatalogOptions.SectionName));
        services.AddSingleton<IBibleTranslationCatalog, BibleTranslationCatalog>();
        services.AddSingleton<BibleTranslationReadService>();

        return services;
    }

    public static IEndpointRouteBuilder MapBiblesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/bibles", (BibleTranslationReadService service) => Results.Ok(service.GetTranslations()))
            .WithName("GetBibles")
            .WithOpenApi();

        return endpoints;
    }
}




