using Application.Abstractions;
using Application.Dtos;
using Application.Queries;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Handlers
{
    public class GetHistoryQueryHandler(ILogger<GetHistoryQueryHandler> logger, ISyncRepository repo) : IRequestHandler<GetHistoryQuery, GetHistoryQueryPagedResponse>
    {
        public async Task<GetHistoryQueryPagedResponse> Handle(GetHistoryQuery request, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Handling GetHistoryQuery with filter: {@Filter}", request.Request);

            var histories = await repo.GetExecutionHistoryAsync(
                new Paginator<HistorySortBy>(
                    request.Request.SortBy,
                    request.Request.SortOrder,
                    request.Request.Offset,
                    request.Request.Limit
                ),
                cancellationToken
            );
            
            return new GetHistoryQueryPagedResponse(
                histories.Count, 
                histories.Histories.Select(GetHistoryQueryResponse.FromDomain).ToList()
            );
        }
    }
}