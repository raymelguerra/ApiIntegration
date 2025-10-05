using MediatR;

namespace Application.Commands.MerchandiseEntry
{
    public record UpdateMerchandiseEntryCommand(string JobKey) : IRequest<Unit>;
}
