using Microsoft.EntityFrameworkCore;
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
            // Test database connectivity
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

            if (!canConnect)
            {
                _logger.LogWarning("PostgreSQL database connection failed");
                return HealthCheckResult.Unhealthy("Cannot connect to PostgreSQL database");
            }

            // Perform a simple query to verify database functionality
            var userCount = await _dbContext.Users.CountAsync(cancellationToken);
            var documentCount = await _dbContext.Documents.CountAsync(cancellationToken);

            var data = new Dictionary<string, object>
            {
                { "database_name", _dbContext.Database.GetDbConnection().Database },
                { "user_count", userCount },
                { "document_count", documentCount },
                { "connection_state", _dbContext.Database.GetDbConnection().State.ToString() }
            };

            _logger.LogDebug("PostgreSQL health check passed with {UserCount} users and {DocumentCount} documents", 
                userCount, documentCount);
            
            return HealthCheckResult.Healthy("PostgreSQL database is healthy and responsive", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PostgreSQL health check failed");
            return HealthCheckResult.Unhealthy($"PostgreSQL health check failed: {ex.Message}");
        }
    }
}