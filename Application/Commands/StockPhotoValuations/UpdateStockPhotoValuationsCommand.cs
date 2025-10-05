

using Application.Abstractions;

namespace Application.Commands.StockPhotoValuations
{
    public record UpdateStockPhotoValuationsCommand(string JobKey) : IRequest<Unit>;
}
