namespace Domain.Entities
{
    public class ExecutionHistory
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public required string JobKey { get; init; }
        public DateTime StartedAtUtc { get; init; }
        public DateTime FinishedAtUtc { get; init; }
        public int ExtractedCount { get; init; }
        public int SuccessCount { get; init; }
        public int FailedCount { get; init; }
        public string? Summary { get; init; }
    }
}