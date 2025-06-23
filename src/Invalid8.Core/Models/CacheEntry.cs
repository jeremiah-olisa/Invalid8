namespace Invalid8.Core;

/// <summary>
/// Represents a cached entry with value, metadata, and expiration information
/// </summary>
/// <typeparam name="T">The type of the cached value</typeparam>
public class CacheEntry<T>
{
    /// <summary>
    /// The cached value
    /// </summary>
    public required T Value { get; set; }
    
    /// <summary>
    /// When this entry was created and cached
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When this entry was last accessed
    /// </summary>
    public DateTime LastAccessed { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When this entry becomes stale (background refresh may occur)
    /// </summary>
    public DateTime? StaleAt { get; set; }
    
    /// <summary>
    /// When this entry expires and must be refetched
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// ETag for concurrency control and conditional requests
    /// </summary>
    public string? ETag { get; set; }
    
    /// <summary>
    /// Tags for categorical invalidation
    /// </summary>
    public string[] Tags { get; set; } = [];
    
    /// <summary>
    /// Number of times this entry has been accessed
    /// </summary>
    public int AccessCount { get; set; }
    
    /// <summary>
    /// Size of the cached data in bytes (approximate)
    /// </summary>
    public long SizeBytes { get; set; }
    
    /// <summary>
    /// Priority level for eviction policies
    /// </summary>
    public CacheEntryPriority Priority { get; set; } = CacheEntryPriority.Normal;
    
    /// <summary>
    /// Gets whether this entry is currently stale
    /// </summary>
    public bool IsStale => StaleAt.HasValue && DateTime.UtcNow >= StaleAt.Value;
    
    /// <summary>
    /// Gets whether this entry has expired
    /// </summary>
    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow >= ExpiresAt.Value;
    
    /// <summary>
    /// Gets whether this entry is still valid (not expired)
    /// </summary>
    public bool IsValid => !IsExpired;
}