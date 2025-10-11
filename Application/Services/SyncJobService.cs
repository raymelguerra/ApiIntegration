using Application.Abstractions;
using Application.Commands;
using Domain.Enums;
using Domain.Extensions;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class SyncJobService(ISender mediator, ILogger<SyncJobService> logger) : ISyncJobService
    {

        public async Task ExecuteJobAsync(string jobKey, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Executing job {JobKey}", jobKey);

                if (!jobKey.TryParseJobKey(out var jobType))
                {
                    logger.LogWarning("Invalid job key: {jobKey}", jobKey);
                    return;
                }

                switch (jobType)
                {
                    case JobType.UpdateProviders:
                        await mediator.Send(new UpdateProvidersCommand(jobKey), cancellationToken);
                        break;

                    case JobType.UpdateMaterials:
                        await mediator.Send(new UpdateMaterialsCommand(jobKey), cancellationToken);
                        break;
                    
                    case JobType.UpdateMerchandiseEntry:
                        await mediator.Send(new UpdateMerchandiseEntryCommand(jobKey), cancellationToken);
                        break;
                    
                    case JobType.UpdateStockPhotoValuations:
                        await mediator.Send(new UpdateStockPhotoValuationsCommand(jobKey), cancellationToken);
                        break;
                    
                    case JobType.UpdateWarehouses:
                        await mediator.Send(new UpdateWarehousesCommand(jobKey), cancellationToken);
                        break;
                    
                    default:
                        logger.LogWarning("No handler for job {jobKey}", jobKey);
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Job {jobKey} failed", jobKey);
                throw;
            }
        }
    }
}