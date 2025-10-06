using Application.Abstractions;
using Application.Dtos;

namespace Application.Queries
{
    public record GetHistoryQuery(GetHistoryQueryFilter Request) : IRequest<GetHistoryQueryPagedResponse>;
}