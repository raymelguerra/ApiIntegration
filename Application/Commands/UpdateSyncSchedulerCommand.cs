using Application.Abstractions;
using Application.Dtos;

namespace Application.Commands
{
    public record UpdateSyncSchedulerCommand(SyncSchedulerUpdateRequest Request) : IRequest<Unit>;
}