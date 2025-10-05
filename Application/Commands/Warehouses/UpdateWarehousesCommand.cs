using MediatR;

namespace Application.Commands.Warehouses
{
    public record UpdateWarehousesCommand(string JobKey) : IRequest<Unit>;
}
