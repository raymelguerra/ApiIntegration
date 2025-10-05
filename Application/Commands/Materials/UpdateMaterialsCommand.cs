using Application.Abstractions;

namespace Application.Commands.Materials
{
    public record UpdateMaterialsCommand(string JobKey) : IRequest<Unit>;
}
