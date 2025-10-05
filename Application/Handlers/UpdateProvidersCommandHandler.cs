using Application.Abstractions;
using Application.Commands;
using Microsoft.Extensions.Logging;

namespace Application.Handlers
{
    public class UpdateProvidersCommandHandler(ILogger<UpdateProvidersCommandHandler> logger) : IRequestHandler<UpdateProvidersCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateProvidersCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Handling UpdateProvidersCommand for job: {JobKey}", request.JobKey);
            
            await Task.Delay(1000, cancellationToken); // Simular trabajo
        
            logger.LogInformation("Completed UpdateProvidersCommand for job: {JobKey}", request.JobKey);
            return Unit.Value;
        }
    }
}