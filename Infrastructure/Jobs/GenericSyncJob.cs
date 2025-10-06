using Domain.Interfaces;
using Infrastructure.Exceptions;
using Infrastructure.Policies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Quartz;

namespace Infrastructure.Jobs
{
    public class GenericSyncJob(IServiceProvider sp, ILogger<GenericSyncJob> log) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var jobKey = context.JobDetail.Key.Name;
            using var scope = sp.CreateScope();
            var cts = new CancellationTokenSource(TimeSpan.FromMinutes(10));
            
            try
            {
                log.LogInformation("Starting job {JobKey}", jobKey);
                
                var retryPolicy = ResiliencePolicies.GetQuartzJobRetryPolicy(log, jobKey);
                var timeoutPolicy = ResiliencePolicies.GetTimeoutPolicy(TimeSpan.FromMinutes(10));
                var combinedPolicy = Policy.WrapAsync(timeoutPolicy, retryPolicy);

                await combinedPolicy.ExecuteAsync(async () =>
                {
                    var jobService = scope.ServiceProvider.GetRequiredService<ISyncJobService>();
                    await jobService.ExecuteJobAsync(jobKey, cts.Token);
                });
            
                log.LogInformation("Completed job {JobKey}", jobKey);
            }
            catch (Exception ex) when (ex.GetType().Name == "ValidationException")
            {
                log.LogError(ex, "Validation error in job {JobKey}", jobKey);
                throw new QuartzJobException(jobKey, "Validation failed", ex);
            }
            catch (Exception ex) when (ex.GetType().Name == "BusinessRuleValidationException")
            {
                log.LogError(ex, "Business rule violation in job {JobKey}", jobKey);
                throw new QuartzJobException(jobKey, "Business rule violation", ex);
            }
            catch (ExternalApiException ex)
            {
                log.LogError(ex, "External API error in job {JobKey}: {ApiName}", jobKey, ex.ApiName);
                throw new QuartzJobException(jobKey, $"External API '{ex.ApiName}' failed", ex);
            }
            catch (DatabaseException ex)
            {
                log.LogError(ex, "Database error in job {JobKey}", jobKey);
                throw new QuartzJobException(jobKey, "Database operation failed", ex);
            }
            catch (Polly.Timeout.TimeoutRejectedException ex)
            {
                log.LogError(ex, "Job {JobKey} timed out", jobKey);
                throw new QuartzJobException(jobKey, "Job execution timed out", ex);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Job {JobKey} failed with unexpected error", jobKey);
                throw new QuartzJobException(jobKey, "Unexpected error occurred", ex);
            }
        }
    }
}