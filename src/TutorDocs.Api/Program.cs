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
builder.Services.AddHealthChecks()
    .AddCheck<TutorDocsPostgresHealthCheck>("postgresHealthCheck");

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
app.MapDocumentEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapTestEndpoints();
}

app.Run();