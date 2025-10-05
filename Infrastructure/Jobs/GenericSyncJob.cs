using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
                log.LogInformation("Starting job {jobKey}", jobKey);
                
                var jobService = scope.ServiceProvider.GetRequiredService<ISyncJobService>();
                await jobService.ExecuteJobAsync(jobKey, cts.Token);
            
                log.LogInformation("Completed job {jobKey}", jobKey);
                
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Job {jobKey} failed", jobKey);
                throw;
            }
        }
    }
}