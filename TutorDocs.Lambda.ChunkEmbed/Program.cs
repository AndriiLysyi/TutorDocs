using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using TutorDocs.Shared.Extensions;
using TutorDocs.Lambda.ChunkEmbed;
Console.WriteLine("Hello, World!");
// try
// {
//     var builder = Host.CreateApplicationBuilder(args);
//     
//     // Add configuration
//     builder.Configuration
//         .AddJsonFile("appsettings.json", optional: false)
//         .AddEnvironmentVariables();
//
//     // Add services
//     builder.Services.AddTutorDocsShared(builder.Configuration);
//     builder.Services.AddScoped<ChunkEmbedHandler>();
//     
//     var host = builder.Build();
//     
//     // Run the handler
//     var handler = host.Services.GetRequiredService<ChunkEmbedHandler>();
//    // await handler.HandleAsync(args);
//     
//     Log.Information("ChunkEmbed Lambda completed successfully");
// }
// catch (Exception ex)
// {
//     Log.Fatal(ex, "ChunkEmbed Lambda failed");
//     Environment.Exit(1);
// }
// finally
// {
//     Log.CloseAndFlush();
// }