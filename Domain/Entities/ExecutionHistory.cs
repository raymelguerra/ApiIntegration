namespace Domain.Entities
{
    public class ExecutionHistory
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string JobKey { get; set; }
        public DateTime StartedAtUtc { get; set; }
        public DateTime FinishedAtUtc { get; set; }
        public int ExtractedCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public string? Summary { get; set; }  // optional message
    }
}