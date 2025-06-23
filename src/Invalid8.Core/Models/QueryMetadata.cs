namespace Invalid8.Core;

/// <summary>
/// Metadata about a cached query for monitoring and debugging
/// </summary>
public class QueryMetadata
{
    /// <summary>
    /// The query key that identifies this cached query
    /// </summary>
    public required string[] Key { get; set; }
    
    /// <summary>
    /// When the query was first executed and cached
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// When the query was last executed
    /// </summary>
    public DateTime LastFetched { get; set; }
    
    /// <summary>
    /// When the cached data becomes stale
    /// </summary>
    public DateTime? StaleAt { get; set; }
    
    /// <summary>
    /// When the cached data expires
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Number of times this query has been executed
    /// </summary>
    public int FetchCount { get; set; }
    
    /// <summary>
    /// Number of times this query has been served from cache
    /// </summary>
    public int CacheHitCount { get; set; }
    
    /// <summary>
    /// Average fetch duration in milliseconds
    /// </summary>
    public double AverageFetchDurationMs { get; set; }
    
    /// <summary>
    /// Last error that occurred during query execution, if any
    /// </summary>
    public Exception? LastError { get; set; }
    
    /// <summary>
    /// When the last error occurred
    /// </summary>
    public DateTime? LastErrorAt { get; set; }
    
    /// <summary>
    /// Gets the cache hit ratio (hits / total accesses)
    /// </summary>
    public double CacheHitRatio => FetchCount > 0 ? (double)CacheHitCount / FetchCount : 0;
    
    /// <summary>
    /// Gets whether the cached data is currently stale
    /// </summary>
    public bool IsStale => StaleAt.HasValue && DateTime.UtcNow >= StaleAt.Value;
    
    /// <summary>
    /// Gets whether the cached data has expired
    /// </summary>
    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow >= ExpiresAt.Value;
}