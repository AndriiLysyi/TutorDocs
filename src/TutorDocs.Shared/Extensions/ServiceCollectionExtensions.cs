using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TutorDocs.Shared.Data;
using TutorDocs.Shared.Repositories;
using TutorDocs.Shared.Services;

namespace TutorDocs.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTutorDocsShared(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<TutorDocsDbContext>(options =>
            options.UseNpgsql(connectionString));
        
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();

        return services;
    }
}