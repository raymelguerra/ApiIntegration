

using Application.Abstractions;

namespace Application.Commands
{
    public record UpdateWarehousesCommand(string JobKey) : IRequest<Unit>;
}
