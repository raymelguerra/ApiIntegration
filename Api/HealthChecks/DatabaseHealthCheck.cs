using Domain.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Api.HealthChecks
{
    /// <summary>
    /// Health check for database connectivity and operations
    /// Verifies that the database is accessible and can perform basic queries
    /// </summary>
    public class DatabaseHealthCheck(ISyncRepository repository, ILogger<DatabaseHealthCheck> logger) : IHealthCheck
    {

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var startTime = DateTime.UtcNow;
                
                // Try to perform a simple database query
                var schedules = await repository.GetScheduleAsync(cancellationToken);
                
                var responseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                var scheduleCount = schedules.Count();
                
                var data = new Dictionary<string, object>
                {
                    { "responseTimeMs", Math.Round(responseTime, 2) },
                    { "scheduleCount", scheduleCount },
                    { "timestamp", DateTime.UtcNow }
                };

                // Determine health status based on response time
                if (responseTime < 1000)
                {
                    logger.LogInformation(
                        "Database health check passed. Response time: {ResponseTime}ms, Schedules: {Count}", 
                        Math.Round(responseTime, 2), 
                        scheduleCount);
                    
                    return HealthCheckResult.Healthy(
                        $"Database is healthy. Response time: {Math.Round(responseTime, 2)}ms", 
                        data);
                }
                else if (responseTime < 3000)
                {
                    logger.LogWarning(
                        "Database health check degraded. Response time: {ResponseTime}ms", 
                        Math.Round(responseTime, 2));
                    
                    return HealthCheckResult.Degraded(
                        $"Database is responding slowly. Response time: {Math.Round(responseTime, 2)}ms", 
                        data: data);
                }
                else
                {
                    logger.LogError(
                        "Database health check unhealthy. Response time: {ResponseTime}ms", 
                        Math.Round(responseTime, 2));
                    
                    return HealthCheckResult.Unhealthy(
                        $"Database is too slow. Response time: {Math.Round(responseTime, 2)}ms", 
                        data: data);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database health check failed with exception");
                
                return HealthCheckResult.Unhealthy(
                    "Database is not accessible or query failed", 
                    ex,
                    new Dictionary<string, object>
                    {
                        { "error", ex.Message },
                        { "timestamp", DateTime.UtcNow }
                    });
            }
        }
    }
}

