using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Api.HealthChecks
{
    /// <summary>
    /// Health check for API general health
    /// Verifies that the API is running and responding
    /// </summary>
    public class ApiHealthCheck(ILogger<ApiHealthCheck> logger, IConfiguration configuration) : IHealthCheck
    {

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Check basic API health
                var appName = configuration["ApplicationName"] ?? "ApiIntegration";
                var environment = configuration["Environment"] ?? "Production";
                var uptime = DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime();
                
                var data = new Dictionary<string, object>
                {
                    { "application", appName },
                    { "environment", environment },
                    { "uptime", $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m" },
                    { "timestamp", DateTime.UtcNow },
                    { "machineName", Environment.MachineName },
                    { "osVersion", Environment.OSVersion.ToString() },
                    { "processorCount", Environment.ProcessorCount },
                    { "workingSet", $"{Environment.WorkingSet / 1024 / 1024} MB" }
                };

                logger.LogInformation("API health check passed. Uptime: {Uptime}", data["uptime"]);
                
                return Task.FromResult(
                    HealthCheckResult.Healthy(
                        "API is running and healthy", 
                        data));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "API health check failed");
                return Task.FromResult(
                    HealthCheckResult.Unhealthy(
                        "API health check failed", 
                        ex));
            }
        }
    }
}

