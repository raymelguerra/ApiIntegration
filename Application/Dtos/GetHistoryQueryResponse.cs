namespace Application.Dtos;

/// <summary>
/// Represents a single execution history record
/// </summary>
public record GetHistoryQueryResponse(
    /// <summary>
    /// Unique identifier for the execution history record
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    Guid Id,
    
    /// <summary>
    /// The job key/identifier that was executed
    /// </summary>
    /// <example>UpdateMaterials</example>
    string JobKey,
    
    /// <summary>
    /// UTC timestamp when the job execution started
    /// </summary>
    /// <example>2025-10-06T10:30:00Z</example>
    DateTime StartedAtUtc,
    
    /// <summary>
    /// UTC timestamp when the job execution finished
    /// </summary>
    /// <example>2025-10-06T10:35:00Z</example>
    DateTime FinishedAtUtc,
    
    /// <summary>
    /// Total number of items extracted during execution
    /// </summary>
    /// <example>150</example>
    int ExtractedCount,
    
    /// <summary>
    /// Number of items successfully processed
    /// </summary>
    /// <example>145</example>
    int SuccessCount,
    
    /// <summary>
    /// Number of items that failed processing
    /// </summary>
    /// <example>5</example>
    int FailedCount,
    
    /// <summary>
    /// Additional metadata or summary information about the execution
    /// </summary>
    /// <example>Sync completed with minor errors</example>
    string? Metadata
)
{
    public static GetHistoryQueryResponse FromDomain(Domain.Entities.ExecutionHistory history) => new(
    history.Id,
    history.JobKey,
    history.StartedAtUtc,
    history.FinishedAtUtc,
    history.ExtractedCount,
    history.SuccessCount,
    history.FailedCount,
    history.Summary
    );
}

/// <summary>
/// Paginated response containing execution history records
/// </summary>
public record GetHistoryQueryPagedResponse(
    /// <summary>
    /// Total number of records available (across all pages)
    /// </summary>
    /// <example>250</example>
    int TotalCount,
    
    /// <summary>
    /// List of execution history items for the current page
    /// </summary>
    List<GetHistoryQueryResponse> Items
);