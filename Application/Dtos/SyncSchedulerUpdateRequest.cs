namespace Application.Dtos
{
    public record SyncSchedulerUpdateRequest
    {
        public required string JobKey { get; init; }
        public string? CronExpression { get; init; }
        public bool? Enabled { get; init; }
        public DateTime? NextExecutionUtc { get; init; }
    }
}