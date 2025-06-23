namespace Invalid8.Core;

/// <summary>
/// Reasons for cache invalidation
/// </summary>
public enum InvalidationReason
{
    /// <summary>
    /// Manual invalidation by user code
    /// </summary>
    Manual,
    
    /// <summary>
    /// Automatic invalidation due to expiration
    /// </summary>
    Expired,
    
    /// <summary>
    /// Invalidation due to dependency change
    /// </summary>
    Dependency,
    
    /// <summary>
    /// Invalidation due to cache eviction policy
    /// </summary>
    Eviction,
    
    /// <summary>
    /// Invalidation due to data corruption
    /// </summary>
    Corruption,
    
    /// <summary>
    /// Invalidation due to tag-based rules
    /// </summary>
    Tag,
    
    /// <summary>
    /// Invalidation due to background refresh
    /// </summary>
    Refresh
}