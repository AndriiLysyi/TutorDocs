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
        var connectionString = BuildConnectionString(configuration);
        services.AddDbContext<TutorDocsDbContext>(options =>
            options.UseNpgsql(connectionString));
        
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();

        return services;
    }

    private static string BuildConnectionString(IConfiguration configuration)
    {
        var baseConnectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrEmpty(baseConnectionString))
        {
            throw new InvalidOperationException("DefaultConnection string is not configured.");
        }
        
        // Try Docker secrets path first (when running in container)
        const string dockerSecretPath = "/run/secrets/postgres_password";
        if (File.Exists(dockerSecretPath))
        {
            var password = File.ReadAllText(dockerSecretPath).Trim();
            return $"{baseConnectionString};Password={password}";
        }
        
        // Fall back to local secrets file (for local development)
        const string localSecretPath = "./secrets/postgres_password.txt";
        if (File.Exists(localSecretPath))
        {
            var password = File.ReadAllText(localSecretPath).Trim();
            return $"{baseConnectionString};Password={password}";
        }
        
        throw new InvalidOperationException(
            "No password file found. Create ./secrets/postgres_password.txt for local development.");
    }
}