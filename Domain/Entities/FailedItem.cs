namespace Domain.Entities
{
    public class FailedItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ExecutionHistoryId { get; set; }
        public string? ItemIdentifier { get; set; }      // optional id
        public string? Reason { get; set; }
        public string? Payload { get; set; }             // store as JSON text
    }
}