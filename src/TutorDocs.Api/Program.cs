using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TutorDocs.Api.Endpoints;
using TutorDocs.Shared.Data;
using TutorDocs.Shared.Extensions;
using TutorDocs.Shared.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddTutorDocsShared(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.Http);
    });
}
app.MapHealthChecks("/healthz");
app.MapHealthChecks("/healthz/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = async (context, report) =>
    {
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                exception = entry.Value.Exception?.Message,
                duration = entry.Value.Duration.ToString(),
                data = entry.Value.Data
            }),
            totalDuration = report.TotalDuration.ToString()
        };
        
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
});
app.MapDocumentEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapTestEndpoints();
}

app.Run();