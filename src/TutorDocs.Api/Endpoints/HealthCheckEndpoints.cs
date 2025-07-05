using Grpc.Core;

namespace TutorDocs.Api.Endpoints;

public static class HealthCheckEndpoints
{
    public static void MapHealthCheckEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/healthcheck")
            .WithTags("HealthCheck")
            .WithOpenApi();

        group.MapGet("/", HealthCheckApi)
            .WithName("HealthCheckApi")
            .WithSummary("HealthCheck for Api")
            .Produces<StatusCode>(200)
            .Produces<StatusCode>(500);
    }

    private static IResult HealthCheckApi()
    {
        return Results.Ok();
    }
}