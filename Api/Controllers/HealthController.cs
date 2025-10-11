using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers
{
    /// <summary>
    /// Health Check Controller
    /// Provides endpoints to monitor the health status of the API and its dependencies
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class HealthController(HealthCheckService healthCheckService, ILogger<HealthController> logger) : ControllerBase
    {

        /// <summary>
        /// Get overall health status
        /// </summary>
        /// <returns>Health status of all components</returns>
        /// <response code="200">Returns healthy status</response>
        /// <response code="503">Returns unhealthy status</response>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Check overall health",
            Description = "Returns the health status of the API and all its dependencies including database",
            Tags = new[] { "Health" }
        )]
        [ProducesResponseType(typeof(HealthReport), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(HealthReport), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetHealth()
        {
            var report = await healthCheckService.CheckHealthAsync();
            
            logger.LogInformation(
                "Health check requested. Status: {Status}, Duration: {Duration}ms",
                report.Status,
                report.TotalDuration.TotalMilliseconds);

            return report.Status == HealthStatus.Healthy
                ? Ok(new
                {
                    status = report.Status.ToString(),
                    totalDuration = $"{report.TotalDuration.TotalMilliseconds}ms",
                    checks = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description,
                        duration = $"{e.Value.Duration.TotalMilliseconds}ms",
                        data = e.Value.Data
                    })
                })
                : StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    status = report.Status.ToString(),
                    totalDuration = $"{report.TotalDuration.TotalMilliseconds}ms",
                    checks = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description,
                        error = e.Value.Exception?.Message,
                        duration = $"{e.Value.Duration.TotalMilliseconds}ms",
                        data = e.Value.Data
                    })
                });
        }

        /// <summary>
        /// Check if API is ready to accept requests
        /// </summary>
        /// <returns>Readiness status</returns>
        /// <response code="200">API is ready</response>
        /// <response code="503">API is not ready</response>
        [HttpGet("ready")]
        [SwaggerOperation(
            Summary = "Check readiness",
            Description = "Returns whether the API is ready to accept requests (all critical dependencies are available)",
            Tags = new[] { "Health" }
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetReadiness()
        {
            var report = await healthCheckService.CheckHealthAsync(check => check.Tags.Contains("ready"));
            
            return report.Status == HealthStatus.Healthy
                ? Ok(new { status = "Ready", message = "API is ready to accept requests" })
                : StatusCode(StatusCodes.Status503ServiceUnavailable, 
                    new { status = "NotReady", message = "API is not ready", details = report.Status.ToString() });
        }

        /// <summary>
        /// Check if API is alive
        /// </summary>
        /// <returns>Liveness status</returns>
        /// <response code="200">API is alive</response>
        [HttpGet("live")]
        [SwaggerOperation(
            Summary = "Check liveness",
            Description = "Returns whether the API process is running (simple ping)",
            Tags = new[] { "Health" }
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetLiveness()
        {
            return Ok(new 
            { 
                status = "Alive", 
                timestamp = DateTime.UtcNow,
                message = "API is alive and responding"
            });
        }

        /// <summary>
        /// Get database health status
        /// </summary>
        /// <returns>Database health status</returns>
        /// <response code="200">Database is healthy</response>
        /// <response code="503">Database is unhealthy</response>
        [HttpGet("database")]
        [SwaggerOperation(
            Summary = "Check database health",
            Description = "Returns the health status of the database connection and query performance",
            Tags = new[] { "Health" }
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetDatabaseHealth()
        {
            var report = await healthCheckService.CheckHealthAsync(check => check.Tags.Contains("database"));
            
            var dbChecks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = $"{e.Value.Duration.TotalMilliseconds}ms",
                data = e.Value.Data,
                error = e.Value.Exception?.Message
            }).ToList();

            return report.Status == HealthStatus.Healthy
                ? Ok(new { status = "Healthy", checks = dbChecks })
                : StatusCode(StatusCodes.Status503ServiceUnavailable, 
                    new { status = report.Status.ToString(), checks = dbChecks });
        }
    }
}

