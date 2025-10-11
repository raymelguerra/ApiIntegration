using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Application.Dtos
{
    /// <summary>
    /// Request to update synchronization scheduler configuration
    /// </summary>
    public record SyncSchedulerUpdateRequest
    {
        /// <summary>
        /// The unique key identifying the scheduled job
        /// </summary>
        /// <example>UpdateMaterials</example>
        [Required]
        public required string JobKey { get; init; }
        
        /// <summary>
        /// Cron expression defining when the job should run (optional)
        /// </summary>
        /// <example>0 0 2 * * ?</example>
        /// <remarks>
        /// Format: Second Minute Hour DayOfMonth Month DayOfWeek
        /// Example: "0 0 2 * * ?" runs every day at 2:00 AM
        /// </remarks>
        public string? CronExpression { get; init; }
        
        /// <summary>
        /// Whether the scheduled job is enabled (optional)
        /// </summary>
        /// <example>true</example>
        public bool? Enabled { get; init; }
        
        /// <summary>
        /// Schedule a one-time execution at the specified UTC time (optional)
        /// </summary>
        /// <example>2025-10-07T02:00:00Z</example>
        public DateTime? NextExecutionUtc { get; init; }
    }
}