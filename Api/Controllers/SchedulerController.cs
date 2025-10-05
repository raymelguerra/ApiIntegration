using Application.Abstractions;
using Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchedulerController(ISender mediator) : ControllerBase
    {
        [HttpPatch("update")]
        public async Task<IActionResult> UpdateScheduler([FromBody] SyncSchedulerUpdateRequest request, CancellationToken ct = default)
        {
            await mediator.Send(new Application.Commands.UpdateSyncSchedulerCommand(request), ct);
            return Ok();
        }

    }
}
