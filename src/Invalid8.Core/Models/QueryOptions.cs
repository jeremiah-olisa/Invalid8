namespace Invalid8.Core;

/// <summary>
/// Options for configuring query behavior and caching
/// </summary>
public class QueryOptions
{
    /// <summary>
    /// Time after which data is considered stale (background refresh may occur)
    /// </summary>
    public TimeSpan? StaleTime { get; set; } = TimeSpan.FromMinutes(5);
    
    /// <summary>
    /// Time after which data expires and must be refetched
    /// </summary>
    public TimeSpan? CacheTime { get; set; } = TimeSpan.FromMinutes(30);
    
    /// <summary>
    /// Number of retry attempts for query failures
    /// </summary>
    public int RetryCount { get; set; } = 3;
    
    /// <summary>
    /// Delay between retry attempts
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
    
    /// <summary>
    /// Whether to enable background refetch of stale data (default: true)
    /// </summary>
    public bool EnableBackgroundRefetch { get; set; } = true;
    
    /// <summary>
    /// Whether to throw exceptions on query failure (default: true)
    /// </summary>
    public bool ThrowOnError { get; set; } = true;
    
    /// <summary>
    /// Timeout for the query operation
    /// </summary>
    public TimeSpan? Timeout { get; set; }
    
    /// <summary>
    /// Tags for categorical invalidation
    /// </summary>
    public string[] Tags { get; set; } = [];
    
    /// <summary>
    /// Priority level for cache eviction
    /// </summary>
    public CacheEntryPriority Priority { get; set; } = CacheEntryPriority.Normal;
}