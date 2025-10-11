namespace Domain.Entities
{
    public class FailedItem
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid ExecutionHistoryId { get; init; }
        public string? ItemIdentifier { get; init; }
        public string? Reason { get; init; }
        public string? Payload { get; init; }
    }
}