using MediatR;

namespace Application.Commands.StockPhotoValuations
{
    public record UpdateStockPhotoValuationsCommand(string JobKey) : IRequest<Unit>;
}
