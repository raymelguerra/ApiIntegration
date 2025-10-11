using Microsoft.Extensions.Diagnostics.HealthChecks;

using Quartz;
using Quartz.Impl.Matchers;

namespace Api.HealthChecks;

/// <summary>
/// Health check for Quartz scheduler status
/// </summary>
public class QuartzHealthCheck : IHealthCheck
{
    private readonly ILogger<QuartzHealthCheck> _logger;
    private readonly ISchedulerFactory _schedulerFactory;

    public QuartzHealthCheck(ISchedulerFactory schedulerFactory, ILogger<QuartzHealthCheck> logger)
    {
        _schedulerFactory = schedulerFactory;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IScheduler scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

            if (!scheduler.IsStarted)
            {
                return HealthCheckResult.Unhealthy("Scheduler has not been started");
            }

            if (scheduler.InStandbyMode)
            {
                return HealthCheckResult.Degraded("Scheduler is in standby mode");
            }

            if (scheduler.IsShutdown)
            {
                return HealthCheckResult.Unhealthy("Scheduler has been shut down");
            }

            // Get scheduler metadata
            var metadata = await scheduler.GetMetaData(cancellationToken);
            var runningJobs = await scheduler.GetCurrentlyExecutingJobs(cancellationToken);
            IReadOnlyCollection<JobKey> jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup(), cancellationToken);

            var data = new Dictionary<string, object>
            {
                {
                    "scheduler_name", metadata.SchedulerName
                },
                {
                    "scheduler_instance_id", metadata.SchedulerInstanceId
                },
                {
                    "running_since", metadata.RunningSince?.ToString("O") ?? "N/A"
                },
                {
                    "number_of_jobs_executed", metadata.NumberOfJobsExecuted
                },
                {
                    "currently_executing_jobs", runningJobs.Count
                },
                {
                    "total_jobs_scheduled", jobKeys.Count
                },
                {
                    "thread_pool_size", metadata.ThreadPoolSize
                },
                {
                    "version", metadata.Version
                }
            };

            _logger.LogDebug(
            "Quartz health check passed. Running jobs: {RunningJobs}, Total jobs: {TotalJobs}",
            runningJobs.Count,
            jobKeys.Count);

            return HealthCheckResult.Healthy(
            $"Scheduler running. Active jobs: {runningJobs.Count}/{jobKeys.Count}",
            data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Quartz health check failed");
            return HealthCheckResult.Unhealthy("Scheduler check failed", ex);
        }
    }
}