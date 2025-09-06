using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using TutorDocs.Shared.Data;

namespace TutorDocs.Shared.Services;

public class TutorDocsPostgresHealthCheck : IHealthCheck
{
    private readonly TutorDocsDbContext _dbContext;
    private readonly ILogger<TutorDocsPostgresHealthCheck> _logger;

    public TutorDocsPostgresHealthCheck(
        TutorDocsDbContext dbContext,
        ILogger<TutorDocsPostgresHealthCheck> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

            if (canConnect)
            {
                return HealthCheckResult.Healthy("API and database are healthy");
            }

            _logger.LogWarning("Database connection failed");
            return HealthCheckResult.Unhealthy("Cannot connect to database");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return HealthCheckResult.Unhealthy($"Health check failed: {ex.Message}");
        }
    }
}