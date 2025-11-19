using Wtrfll.Server.Application.Common;
using Wtrfll.Server.Slices.Sessions.Application;
using Wtrfll.Server.Slices.Sessions.Realtime;

namespace Wtrfll.Server.Slices.Sessions.Endpoints;

public static class SessionsModule
{
    public static IServiceCollection AddSessionsModule(this IServiceCollection services)
    {
        services.AddScoped<SessionLifecycleService>();
        services.AddScoped<SessionQueryService>();
        services.AddSingleton<SessionConnectionRegistry>();
        return services;
    }

    public static IEndpointRouteBuilder MapSessionsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/sessions");

        group.MapPost("/", async (CreateSessionRequestDto? request, SessionLifecycleService service, CancellationToken cancellationToken) =>
            {
                var payload = request ?? new CreateSessionRequestDto();
                var session = await service.CreateSessionAsync(payload, cancellationToken);
                return Results.Created($"/api/sessions/{session.Id}", session);
            })
            .WithName("CreateSession")
            .WithOpenApi();

        group.MapPost("/{id:guid}/join", async (Guid id, JoinSessionRequestDto request, SessionLifecycleService service, CancellationToken cancellationToken) =>
            {
                var outcome = await service.JoinSessionAsync(id, request.Role, request.JoinToken, cancellationToken);
                return outcome.Status switch
                {
                    JoinSessionOperationStatus.NotFound => Results.NotFound(new ErrorResponse("Session not found.", $"Session {id} was not located.")),
                    JoinSessionOperationStatus.InvalidToken => Results.BadRequest(new ErrorResponse("Invalid join token.", "Verify the join link or request a new session.")),
                    JoinSessionOperationStatus.ControllerLocked => Results.Conflict(outcome.Payload! with { Ok = false, ControllerLocked = true }),
                    _ => Results.Ok(outcome.Payload),
                };
            })
            .WithName("JoinSession")
            .WithOpenApi();

        group.MapGet("/upcoming", async (SessionQueryService queryService, CancellationToken cancellationToken) =>
            {
                var sessions = await queryService.GetUpcomingSessionsAsync(DateTime.UtcNow, cancellationToken);
                return Results.Ok(sessions);
            })
            .WithName("ListUpcomingSessions")
            .WithOpenApi();

        return endpoints;
    }

    public static IEndpointRouteBuilder MapSessionsRealtimeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHub<SessionHub>("/realtime");
        return endpoints;
    }
}

