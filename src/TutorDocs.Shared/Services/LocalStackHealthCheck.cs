using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace TutorDocs.Shared.Services;

public class LocalStackHealthCheck : IHealthCheck
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<LocalStackHealthCheck> _logger;
    private readonly string _bucketName;

    public LocalStackHealthCheck(
        IAmazonS3 s3Client,
        IConfiguration configuration,
        ILogger<LocalStackHealthCheck> logger)
    {
        _s3Client = s3Client;
        _logger = logger;
        _bucketName = configuration["AWS:BucketName"] ?? "tutordocs-documents";
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Use a timeout for the health check to prevent hanging
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(5)); // 5 second timeout
            
            // Check if we can list buckets (basic connectivity test)
            var bucketsResponse = await _s3Client.ListBucketsAsync(timeoutCts.Token);
            
            // Check if our specific bucket exists
            var bucketExists = bucketsResponse.Buckets.Any(b => b.BucketName == _bucketName);
            
            if (!bucketExists)
            {
                _logger.LogWarning("LocalStack S3 is accessible but bucket '{BucketName}' does not exist", _bucketName);
                return HealthCheckResult.Degraded($"LocalStack S3 accessible but bucket '{_bucketName}' missing");
            }

            // Perform a simple operation to verify full functionality
            var headRequest = new GetBucketLocationRequest
            {
                BucketName = _bucketName
            };
            
            await _s3Client.GetBucketLocationAsync(headRequest, timeoutCts.Token);
            
            var data = new Dictionary<string, object>
            {
                { "bucket_name", _bucketName },
                { "total_buckets", bucketsResponse.Buckets.Count },
                { "service", "LocalStack S3" }
            };

            _logger.LogDebug("LocalStack S3 health check passed for bucket '{BucketName}'", _bucketName);
            return HealthCheckResult.Healthy("LocalStack S3 is healthy and bucket is accessible", data);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("LocalStack S3 health check was cancelled");
            return HealthCheckResult.Unhealthy("LocalStack S3 health check was cancelled");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("LocalStack S3 health check timed out after 5 seconds");
            return HealthCheckResult.Unhealthy("LocalStack S3 health check timed out - service may be unreachable");
        }
        catch (AmazonS3Exception s3Ex) when (s3Ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("LocalStack S3 bucket '{BucketName}' not found: {Message}", _bucketName, s3Ex.Message);
            return HealthCheckResult.Degraded($"LocalStack S3 bucket '{_bucketName}' not found");
        }
        catch (AmazonS3Exception s3Ex)
        {
            _logger.LogError(s3Ex, "LocalStack S3 error during health check: {ErrorCode}", s3Ex.ErrorCode);
            return HealthCheckResult.Unhealthy($"LocalStack S3 error: {s3Ex.ErrorCode} - {s3Ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LocalStack S3 health check failed with unexpected error");
            return HealthCheckResult.Unhealthy($"LocalStack S3 health check failed: {ex.Message}");
        }
    }
}