namespace Invalid8.Core;

/// <summary>
/// Event published when cache entries are updated
/// </summary>
public class CacheUpdatedEvent
{
    /// <summary>
    /// The cache keys that were updated
    /// </summary>
    public required string[][] Keys { get; set; }
    
    /// <summary>
    /// Source of the update (which instance initiated it)
    /// </summary>
    public required string SourceInstance { get; set; }
    
    /// <summary>
    /// Timestamp when the update occurred
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Size of the updated data in bytes (approximate)
    /// </summary>
    public long DataSizeBytes { get; set; }
    
    /// <summary>
    /// Whether this was a background refresh
    /// </summary>
    public bool IsBackgroundRefresh { get; set; }
}