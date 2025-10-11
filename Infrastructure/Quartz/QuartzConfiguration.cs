namespace Infrastructure.Quartz;

/// <summary>
/// Configuration settings for Quartz.NET scheduler
/// </summary>
public class QuartzConfiguration
{
    public const string SectionName = "Quartz";
    
    /// <summary>
    /// Database connection string for Quartz persistent store
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
    
    /// <summary>
    /// Table prefix for Quartz tables in the database
    /// </summary>
    public string TablePrefix { get; init; } = "quartz.qrtz_";
    
    /// <summary>
    /// Whether to validate schema on startup
    /// </summary>
    public bool PerformSchemaValidation { get; init; } = true;
    
    /// <summary>
    /// Whether to wait for jobs to complete on shutdown
    /// </summary>
    public bool WaitForJobsToComplete { get; init; } = true;
    
    /// <summary>
    /// Maximum number of concurrent threads
    /// </summary>
    public int MaxConcurrency { get; init; } = 10;
    
    /// <summary>
    /// Misfire threshold in seconds
    /// </summary>
    public int MisfireThresholdSeconds { get; init; } = 60;
    
    /// <summary>
    /// Enable clustering for horizontal scaling
    /// </summary>
    public bool EnableClustering { get; init; }
    
    /// <summary>
    /// Cluster checkin interval in seconds
    /// </summary>
    public int ClusterCheckinIntervalSeconds { get; init; } = 20;
    
    /// <summary>
    /// Cluster misfire threshold in seconds
    /// </summary>
    public int ClusterMisfireThresholdSeconds { get; init; } = 30;
}

