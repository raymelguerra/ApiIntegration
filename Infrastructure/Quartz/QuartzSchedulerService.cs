using Application.Abstractions;
using Infrastructure.Jobs;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Infrastructure.Quartz
{
    public class QuartzSchedulerService(IScheduler scheduler, ILogger<QuartzSchedulerService> logger) : ISchedulerService
    {
        public async Task ScheduleOneTimeJobAsync(string jobKey, DateTime runAt, CancellationToken ct = default)
        {
            logger.LogInformation("Scheduling job {JobKey}", jobKey);
            
            // find if job already exists
            var jobIdentity = new JobKey(jobKey);
            var existingJob = await scheduler.CheckExists(jobIdentity, ct);
            // if job does not exist, create it
            if (!existingJob)
            {
                var job = JobBuilder.Create<GenericSyncJob>()
                    .WithIdentity(jobKey)
                    .Build();

                var trigger = TriggerBuilder.Create()
                    .WithIdentity($"{jobKey}.trigger")
                    .StartAt(runAt)
                    .ForJob(job)
                    .Build();

                await scheduler.ScheduleJob(job, trigger, ct);
                logger.LogInformation("Scheduled one-time job {JobKey} to run at {RunAt}", jobKey, runAt);
            }
            else
            {
                // verify if there is an existing one time trigger for the job
                var triggers = await scheduler.GetTriggersOfJob(jobIdentity, ct);
                var oneTimeTrigger = triggers.FirstOrDefault(t => t.Key.Name.EndsWith(".OneTime.trigger"));
                if (oneTimeTrigger != null)
                {
                    // if there is an existing one time trigger, delete it
                    await scheduler.UnscheduleJob(oneTimeTrigger.Key, ct);
                    logger.LogInformation("Deleted existing one-time trigger for job {JobKey}", jobKey);
                }
                // create a new one time trigger for the existing job
                var trigger = TriggerBuilder.Create()
                    .WithIdentity($"{jobKey}.OneTime.trigger")
                    .StartAt(runAt)
                    .ForJob(jobKey)
                    .Build();
                
                await scheduler.ScheduleJob(trigger, ct);
                logger.LogInformation("Scheduled one-time trigger for existing job {JobKey} to run at {RunAt}", jobKey, runAt);
            }
        }
        public bool ValidateCronExpression(string cronExpression, CancellationToken ct = default)
        {
            return CronExpression.IsValidExpression(cronExpression);
        }
    }
}