using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.MerchandiseEntry
{
    public class UpdateMerchandiseEntryCommandHandler(ILogger<UpdateMerchandiseEntryCommandHandler> logger) : IRequestHandler<UpdateMerchandiseEntryCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateMerchandiseEntryCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Handling UpdateMerchandiseEntryCommand for job: {JobKey}", request.JobKey);
            
            await Task.Delay(1000, cancellationToken); // Simular trabajo
        
            logger.LogInformation("Completed UpdateMerchandiseEntryCommand for job: {JobKey}", request.JobKey);
            return Unit.Value;
        }
    }
}