namespace Domain.Enums
{
    public class Paginator<T>(HistorySortBy requestSortBy, SortOrder requestSortOrder, int requestOffset, int requestLimit)
    {
        public int? Offset { get; init; } = 0;
        public int? Limit { get; init; } = 25;
        public T? SortBy { get; init; }
        public SortOrder SortOrder { get; init; } = SortOrder.Ascending;
    }
}