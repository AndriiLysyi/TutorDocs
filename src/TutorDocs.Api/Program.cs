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
    .AddCheck<TutorDocsHealthCheck>("tutordocs");

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
app.MapTestEndpoints();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TutorDocsDbContext>();
    await context.Database.MigrateAsync();
}

app.Run();