


using Application.Abstractions;

namespace Application.Commands
{
    public record UpdateProvidersCommand(string JobKey) : IRequest<Unit>;
}
