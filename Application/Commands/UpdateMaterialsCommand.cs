using Application.Abstractions;

namespace Application.Commands
{
    public record UpdateMaterialsCommand(string JobKey) : IRequest<Unit>;
}
