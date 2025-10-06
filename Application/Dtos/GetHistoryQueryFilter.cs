using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.Dtos;

/// <summary>
/// Filter parameters for retrieving execution history with pagination and sorting
/// </summary>
public record GetHistoryQueryFilter(
    /// <summary>
    /// Sort order (Ascending or Descending)
    /// </summary>
    /// <example>Descending</example>
    SortOrder SortOrder,
    
    /// <summary>
    /// Field to sort by
    /// </summary>
    /// <example>StartedAt</example>
    HistorySortBy SortBy,
    
    /// <summary>
    /// Number of items to skip (for pagination)
    /// </summary>
    /// <example>0</example>
    [Range(0, int.MaxValue)]
    int Offset,
    
    /// <summary>
    /// Maximum number of items to return
    /// </summary>
    /// <example>20</example>
    [Range(1, 100)]
    int Limit
);