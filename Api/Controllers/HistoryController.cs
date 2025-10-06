using Application.Abstractions;
using Application.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// Manages execution history records for synchronization jobs
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class HistoryController(ISender mediator) : ControllerBase
    {
        /// <summary>
        /// Retrieves paginated execution history records with filtering and sorting
        /// </summary>
        /// <param name="filter">Filter parameters including sorting and pagination options</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>A paginated list of execution history records</returns>
        /// <response code="200">Returns the paginated execution history</response>
        /// <response code="400">If the request parameters are invalid</response>
        /// <response code="500">If an internal server error occurs</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/history?sortOrder=Descending&amp;sortBy=StartedAt&amp;offset=0&amp;limit=20
        ///     
        /// This endpoint returns execution history with pagination support.
        /// You can sort by different fields and control the page size.
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(GetHistoryQueryPagedResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GetHistoryQueryPagedResponse>> GetHistory(
            [FromQuery] GetHistoryQueryFilter filter, 
            CancellationToken ct = default)
        {
            var response = await mediator.Send(new Application.Queries.GetHistoryQuery(filter), ct);
            return Ok(response);
        }
    }
}
