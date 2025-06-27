using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TutorDocs.Shared.Data;

namespace TutorDocs.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTutorDocsShared(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddDbContext<TutorDocsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        
        //services.AddScoped<IDocumentService, DocumentService>();


        return services;
    }
}