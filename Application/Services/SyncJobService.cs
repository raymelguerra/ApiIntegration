using Application.Commands.Materials;
using Application.Commands.Providers;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class SyncJobService(IMediator mediator, ILogger<SyncJobService> logger) : ISyncJobService
    {

        public async Task ExecuteJobAsync(string jobKey, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Executing job {JobKey}", jobKey);
                switch (jobKey)
                {
                    case "UpdateProviders":
                        await mediator.Send(new UpdateProvidersCommand(jobKey), cancellationToken);
                        break;

                    case "UpdateMaterials":
                        await mediator.Send(new UpdateMaterialsCommand(jobKey), cancellationToken);
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