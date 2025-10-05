using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Warehouses
{
    public class UpdateWarehousesCommandHandler(ILogger<UpdateWarehousesCommandHandler> logger) : IRequestHandler<UpdateWarehousesCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateWarehousesCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Handling UpdateWarehousesCommand for job: {JobKey}", request.JobKey);
            
            await Task.Delay(1000, cancellationToken); // Simular trabajo
        
            logger.LogInformation("Completed UpdateWarehousesCommand for job: {JobKey}", request.JobKey);
            return Unit.Value;
        }
    }
}