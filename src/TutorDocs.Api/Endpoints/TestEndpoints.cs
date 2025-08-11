using Microsoft.EntityFrameworkCore;
using TutorDocs.Shared.Data;
using TutorDocs.Shared.Data.Entities;

namespace TutorDocs.Api.Endpoints;

public static class TestEndpoints
{
    public static void MapTestEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/test")
            .WithTags("Testing")
            .WithOpenApi();

        group.MapGet("/db", TestDatabaseConnection)
            .WithName("TestDatabaseConnection")
            .WithSummary("Test database connectivity and get basic stats")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status500InternalServerError);

        group.MapPost("/user", TestCreateUser)
            .WithName("TestCreateUser")
            .WithSummary("Create a test user to verify database operations")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> TestDatabaseConnection(
        TutorDocsDbContext context,
        ILogger<Program> logger)
    {
        try
        {
            var userCount = await context.Users.CountAsync();
            var documentCount = await context.Documents.CountAsync();
            
            return TypedResults.Ok(new 
            { 
                Message = "Database connection successful",
                UserCount = userCount,
                DocumentCount = documentCount,
                DatabaseName = context.Database.GetDbConnection().Database,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database connection test failed");
            return TypedResults.Problem(
                detail: ex.Message,
                title: "Database connection failed",
                statusCode: 500);
        }
    }

    private static async Task<IResult> TestCreateUser(
        TutorDocsDbContext context,
        ILogger<Program> logger)
    {
        try
        {
            var testUser = new User
            {
                Id = Guid.NewGuid(),
                Email = $"test-{DateTime.UtcNow.Ticks}@example.com",
                Name = "Test User"
            };

            context.Users.Add(testUser);
            await context.SaveChangesAsync();

            return TypedResults.Ok(new 
            { 
                Message = "Test user created successfully",
                UserId = testUser.Id,
                Email = testUser.Email,
                Name = testUser.Name,
                CreatedAt = testUser.CreatedAt
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Test user creation failed");
            return TypedResults.Problem(
                detail: ex.Message,
                title: "Test user creation failed",
                statusCode: 500);
        }
    }
}