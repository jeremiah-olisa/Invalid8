namespace Invalid8.Core;

/// <summary>
/// Options for cache entry behavior and expiration
/// </summary>
public class CacheEntryOptions
{
    /// <summary>
    /// Absolute expiration time for the cache entry
    /// </summary>
    public DateTimeOffset? AbsoluteExpiration { get; set; }
    
    /// <summary>
    /// Relative expiration time from now
    /// </summary>
    public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
    
    /// <summary>
    /// Sliding expiration time (resets on access)
    /// </summary>
    public TimeSpan? SlidingExpiration { get; set; }
    
    /// <summary>
    /// Time after which data is considered stale
    /// </summary>
    public TimeSpan? StaleTime { get; set; }
    
    /// <summary>
    /// Priority level for cache eviction
    /// </summary>
    public CacheEntryPriority Priority { get; set; } = CacheEntryPriority.Normal;
    
    /// <summary>
    /// Tags for categorical invalidation
    /// </summary>
    public string[] Tags { get; set; } = [];
    
    /// <summary>
    /// Calculates the absolute expiration time based on current time
    /// </summary>
    public DateTimeOffset? GetAbsoluteExpiration()
    {
        if (AbsoluteExpiration.HasValue) return AbsoluteExpiration;
        if (AbsoluteExpirationRelativeToNow.HasValue) 
            return DateTimeOffset.UtcNow + AbsoluteExpirationRelativeToNow.Value;
        return null;
    }
}