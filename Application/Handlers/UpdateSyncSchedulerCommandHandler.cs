using Application.Abstractions;
using Application.Commands;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Handlers
{
    public class UpdateSyncSchedulerCommandHandler(ISchedulerService schedulerService, ILogger<UpdateSyncSchedulerCommandHandler> logger, ISyncRepository repo) : IRequestHandler<UpdateSyncSchedulerCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateSyncSchedulerCommand request, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Handling UpdateSyncSchedulerCommand for job: {JobKey}", request.Request.JobKey);

            ArgumentNullException.ThrowIfNull(request);
            
            var existingSchedule = await repo.GetScheduleAsync(request.Request.JobKey, cancellationToken);
            if (existingSchedule == null)
            {
                throw new KeyNotFoundException($"Schedule with JobKey {request.Request.JobKey} not found.");
            }
            
            if (request.Request.CronExpression is not null and {} cron &&
                cron != existingSchedule.CronExpression)
            {
                existingSchedule.CronExpression = request.Request.CronExpression;
            }
            
            if (request.Request.Enabled is not null and {} enabled &&
                enabled != existingSchedule.Enabled)
            {
                existingSchedule.Enabled = request.Request.Enabled.Value;
            }

            if (request.Request.NextExecutionUtc is not null and {} nextExecutionUtc)
            {
                if (!existingSchedule.Enabled)
                {
                    return Unit.Value;
                }
                
                await schedulerService.ScheduleOneTimeJobAsync(request.Request.JobKey, nextExecutionUtc, cancellationToken);
            }


            logger.LogInformation("Completed UpdateSyncSchedulerCommand for job: {JobKey}", request.Request.JobKey);
            return Unit.Value;
        }
    }
}