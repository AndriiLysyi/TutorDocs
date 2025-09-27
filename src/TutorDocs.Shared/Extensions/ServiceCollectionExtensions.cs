using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TutorDocs.Shared.Data;
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
        
        // Configure AWS S3 client with explicit settings for LocalStack
        services.AddSingleton<IAmazonS3>(provider =>
        {
            var config = new AmazonS3Config
            {
                ServiceURL = configuration["AWS:ServiceURL"],
                ForcePathStyle = true,
                Timeout = TimeSpan.FromSeconds(10),
                MaxErrorRetry = 2,
                UseHttp = configuration["AWS:ServiceURL"]?.StartsWith("http://") == true,
                AuthenticationRegion = configuration["AWS:Region"]
            };
            
            var credentials = new Amazon.Runtime.BasicAWSCredentials(
                configuration["AWS:AccessKey"], 
                configuration["AWS:SecretKey"]
            );
            
            return new AmazonS3Client(credentials, config);
        });
        
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IUserProvider, UserProvider>();
        services.AddScoped<IS3Service, S3Service>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Health checks
        services.AddHealthChecks()
            .AddCheck<TutorDocsPostgresHealthCheck>("postgresql", 
                tags: new[] { "database", "postgresql" })
            .AddCheck<LocalStackHealthCheck>("localstack", 
                tags: new[] { "storage", "s3", "localstack" });

        return services;
    }
}