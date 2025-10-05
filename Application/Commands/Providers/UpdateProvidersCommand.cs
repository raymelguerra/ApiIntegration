


using Application.Abstractions;

namespace Application.Commands.Providers
{
    public record UpdateProvidersCommand(string JobKey) : IRequest<Unit>;
}
