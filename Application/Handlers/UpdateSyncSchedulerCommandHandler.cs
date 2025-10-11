using Application.Abstractions;
using Application.Commands;
using Application.Exceptions;
using Domain.Extensions;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Handlers
{
    public class UpdateSyncSchedulerCommandHandler(ISchedulerService schedulerService, ILogger<UpdateSyncSchedulerCommandHandler> logger, ISyncRepository repo) : IRequestHandler<UpdateSyncSchedulerCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateSyncSchedulerCommand request, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Handling UpdateSyncSchedulerCommand for job: {JobKey}", request.Request.JobKey);

            if (request.Request == null)
            {
                throw new ValidationException("Request", "Request cannot be null");
            }
            
            // validate if JobKey is valid
            if (!request.Request.JobKey.IsValidJobKey())
            {
                throw new ValidationException("JobKey", "Invalid JobKey: " + request.Request.JobKey);
            }

            var existingSchedule = await repo.GetScheduleAsync(request.Request.JobKey, cancellationToken);
            
            if (request.Request.CronExpression is not null and {} cron &&
                cron != existingSchedule.CronExpression)
            {
                // validate if cron expression is valid
                if(!schedulerService.ValidateCronExpression(cron, cancellationToken)) 
                {
                    throw new ValidationException("CronExpression", $"Invalid cron expression: {cron}");
                }
                
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
            
            await repo.UpsertScheduleAsync(existingSchedule, cancellationToken);
            await repo.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Completed UpdateSyncSchedulerCommand for job: {JobKey}", request.Request.JobKey);
            return Unit.Value;
        }
    }
}