using Application.Abstractions;
using Application.Commands;
using Microsoft.Extensions.Logging;

namespace Application.Handlers
{
    public class UpdateStockPhotoValuationsCommandHandler(ILogger<UpdateStockPhotoValuationsCommandHandler> logger) : IRequestHandler<UpdateStockPhotoValuationsCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateStockPhotoValuationsCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Handling UpdateStockPhotoValuationsCommand for job: {JobKey}", request.JobKey);
            
            await Task.Delay(1000, cancellationToken); // Simular trabajo
        
            logger.LogInformation("Completed UpdateStockPhotoValuationsCommand for job: {JobKey}", request.JobKey);
            return Unit.Value;
        }
    }
}