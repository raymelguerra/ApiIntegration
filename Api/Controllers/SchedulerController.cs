using Application.Abstractions;
using Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// Manages synchronization job scheduler configurations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class SchedulerController(ISender mediator) : ControllerBase
    {
        /// <summary>
        /// Updates the configuration of a scheduled synchronization job
        /// </summary>
        /// <param name="request">The scheduler update request containing job configuration</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Success status if the update was successful</returns>
        /// <response code="200">Returns success if the scheduler was updated successfully</response>
        /// <response code="400">If the request is invalid or validation fails</response>
        /// <response code="404">If the specified job schedule was not found</response>
        /// <response code="500">If an internal server error occurs</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     PATCH /api/scheduler/update
        ///     {
        ///       "jobKey": "UpdateMaterials",
        ///       "cronExpression": "0 0 2 * * ?",
        ///       "enabled": true,
        ///       "nextExecutionUtc": "2025-10-07T02:00:00Z"
        ///     }
        ///     
        /// This endpoint allows you to update the schedule configuration for sync jobs.
        /// You can modify the cron expression, enable/disable the job, or schedule a one-time execution.
        /// </remarks>
        [HttpPatch("update")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateScheduler(
            [FromBody] SyncSchedulerUpdateRequest request, 
            CancellationToken ct = default)
        {
            await mediator.Send(new Application.Commands.UpdateSyncSchedulerCommand(request), ct);
            return Ok();
        }
    }
}
