using Wtrfll.Server.Slices.BibleBooks.Application;
using Wtrfll.Server.Slices.BibleBooks.Infrastructure;

namespace Wtrfll.Server.Slices.BibleBooks.Endpoints;

public static class BibleBooksModule
{
    public static IServiceCollection AddBibleBooksModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BibleBookMetadataOptions>(configuration.GetSection(BibleBookMetadataOptions.SectionName));
        services.AddSingleton<IBibleBookMetadataStore, FileSystemBibleBookMetadataStore>();
        services.AddSingleton<BibleBookCatalogService>();
        return services;
    }

    public static IEndpointRouteBuilder MapBibleBooksEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/bible-books", (BibleBookCatalogService catalog) =>
            {
                var books = catalog.GetAll();
                return Results.Ok(books);
            })
            .WithName("GetBibleBooks")
            .WithOpenApi();

        return endpoints;
    }
}
