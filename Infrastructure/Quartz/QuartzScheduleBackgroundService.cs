using Domain.Interfaces;
using Infrastructure.Jobs;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Infrastructure.Quartz
{
    public class QuartzScheduleBackgroundService(
        ISchedulerFactory schedulerFactory,
        IServiceProvider sp,
        ILogger<QuartzScheduleBackgroundService> log) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
            
            await scheduler.Start(cancellationToken);

            // Load schedules from DB and create triggers
            using var scope = sp.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<ISyncRepository>();
            
            var schedules = await repo.GetScheduleAsync(cancellationToken);

            foreach (var s in schedules.Where(x => x.Enabled))
            {
                var job = JobBuilder.Create<GenericSyncJob>()
                    .WithIdentity(s.JobKey)
                    .Build();
                
                var triggerKey = new TriggerKey($"{s.JobKey}.trigger");
                var existingTrigger = await scheduler.GetTrigger(triggerKey, cancellationToken);
                if (existingTrigger != null)
                {
                    log.LogInformation("Trigger for job {jobKey} already exists, skipping", s.JobKey);
                    continue;
                }

                var trigger = TriggerBuilder.Create()
                    .WithIdentity($"{s.JobKey}.trigger")
                    .WithCronSchedule(s.CronExpression)
                    .ForJob(job)
                    .Build();

                await scheduler.ScheduleJob(job, trigger, cancellationToken);
                
                log.LogInformation("Scheduled job {jobKey} with cron {cron}", s.JobKey, s.CronExpression);
            }
        }
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
            await scheduler.Shutdown(waitForJobsToComplete: true, cancellationToken: cancellationToken);
        }
    }
}