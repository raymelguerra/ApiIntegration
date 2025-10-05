
using MediatR;

namespace Application.Commands.Providers
{
    public record UpdateProvidersCommand(string JobKey) : IRequest<Unit>;
}
