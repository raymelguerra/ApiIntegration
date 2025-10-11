

using Application.Abstractions;

namespace Application.Commands
{
    public record UpdateStockPhotoValuationsCommand(string JobKey) : IRequest<Unit>;
}
