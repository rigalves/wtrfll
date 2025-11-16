namespace Wtrfll.Server.Modules.Health;

public static class HealthModule
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/health", () => Results.Ok(new { status = "ok", timestamp = DateTimeOffset.UtcNow }))
            .WithName("GetHealth")
            .WithOpenApi();

        return endpoints;
    }
}
