using Application.Abstractions;
using Application.Commands;
using Microsoft.Extensions.Logging;

namespace Application.Handlers
{
    public class UpdateMaterialsCommandHandler(ILogger<UpdateMaterialsCommandHandler> logger) : IRequestHandler<UpdateMaterialsCommand, Unit>
    {

        public async Task<Unit> Handle(UpdateMaterialsCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Handling UpdateMaterialsCommand for job: {JobKey}", request.JobKey);
            
            await Task.Delay(1000, cancellationToken);
        
            logger.LogInformation("Completed UpdateMaterialsCommand for job: {JobKey}", request.JobKey);
            return Unit.Value;
        }
    }
}
