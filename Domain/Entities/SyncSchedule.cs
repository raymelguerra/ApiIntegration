namespace Domain.Entities
{
    public class SyncSchedule
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string JobKey { get; set; }            // e.g. "UpdateProviders"
        public required string CronExpression { get; set; }      // e.g. "0 0 * * *" (every day at midnight)
        public bool Enabled { get; set; } = true;
        public DateTime? LastModifiedUtc { get; set; }
    }
}