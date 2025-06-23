namespace Invalid8.Core;

/// <summary>
/// Priority levels for cache entry eviction
/// </summary>
public enum CacheEntryPriority
{
    /// <summary>
    /// Low priority - first to be evicted
    /// </summary>
    Low,
    
    /// <summary>
    /// Normal priority - default level
    /// </summary>
    Normal,
    
    /// <summary>
    /// High priority - evicted only when necessary
    /// </summary>
    High,
    
    /// <summary>
    /// Never evict - remains in cache until explicit removal
    /// </summary>
    NeverRemove
}