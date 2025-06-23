namespace Invalid8.Core;

/// <summary>
/// Event published when cache entries are invalidated
/// </summary>
public class CacheInvalidationEvent
{
    /// <summary>
    /// The cache keys that were invalidated
    /// </summary>
    public required string[][] Keys { get; set; }
    
    /// <summary>
    /// Reason for invalidation
    /// </summary>
    public required InvalidationReason Reason { get; set; }
    
    /// <summary>
    /// Source of the invalidation (which instance initiated it)
    /// </summary>
    public required string SourceInstance { get; set; }
    
    /// <summary>
    /// Timestamp when the invalidation occurred
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Tags that were invalidated (if using tag-based invalidation)
    /// </summary>
    public string[]? Tags { get; set; }
}