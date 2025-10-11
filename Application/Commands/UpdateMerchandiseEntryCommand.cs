
using Application.Abstractions;

namespace Application.Commands
{
    public record UpdateMerchandiseEntryCommand(string JobKey) : IRequest<Unit>;
}
