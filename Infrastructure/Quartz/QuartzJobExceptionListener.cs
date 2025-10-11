using Microsoft.Extensions.Logging;
using Quartz;

namespace Infrastructure.Quartz;

public class QuartzJobExceptionListener : IJobListener
{
    private readonly ILogger<QuartzJobExceptionListener> _logger;

    public QuartzJobExceptionListener(ILogger<QuartzJobExceptionListener> logger)
    {
        _logger = logger;
    }

    public string Name => "QuartzJobExceptionListener";

    public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Job {JobName} is about to be executed. FireInstanceId: {FireInstanceId}",
            context.JobDetail.Key.Name,
            context.FireInstanceId);
        
        return Task.CompletedTask;
    }

    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "Job {JobName} execution was vetoed. FireInstanceId: {FireInstanceId}",
            context.JobDetail.Key.Name,
            context.FireInstanceId);
        
        return Task.CompletedTask;
    }

    public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException, CancellationToken cancellationToken = default)
    {
        if (jobException != null)
        {
            _logger.LogError(
                jobException,
                "Job {JobName} threw an exception. FireInstanceId: {FireInstanceId}, RefireImmediately: {RefireImmediately}",
                context.JobDetail.Key.Name,
                context.FireInstanceId,
                jobException.RefireImmediately);

            // Store exception details in job data map for later retrieval
            context.JobDetail.JobDataMap.Put("LastExceptionMessage", jobException.Message);
            context.JobDetail.JobDataMap.Put("LastExceptionTime", DateTime.UtcNow);
            context.JobDetail.JobDataMap.Put("LastExceptionType", jobException.GetType().Name);

            // Determine if job should be rescheduled based on exception type
            if (IsTransientError(jobException))
            {
                _logger.LogInformation(
                    "Job {JobName} failed with transient error. Will be retried on next scheduled execution.",
                    context.JobDetail.Key.Name);
            }
            else
            {
                _logger.LogWarning(
                    "Job {JobName} failed with non-transient error. Manual intervention may be required.",
                    context.JobDetail.Key.Name);
            }
        }
        else
        {
            _logger.LogInformation(
                "Job {JobName} completed successfully. FireInstanceId: {FireInstanceId}, RunTime: {RunTime}ms",
                context.JobDetail.Key.Name,
                context.FireInstanceId,
                context.JobRunTime.TotalMilliseconds);
        }

        return Task.CompletedTask;
    }

    private static bool IsTransientError(Exception exception)
    {
        var exceptionType = exception.GetType().Name;
        
        // Consider these as transient errors that can be retried
        return exceptionType.Contains("Timeout") ||
               exceptionType.Contains("Http") ||
               exceptionType.Contains("Network") ||
               exceptionType.Contains("Connection") ||
               exception.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
               exception.Message.Contains("unavailable", StringComparison.OrdinalIgnoreCase);
    }
}

