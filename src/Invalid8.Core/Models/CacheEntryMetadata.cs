namespace Invalid8.Core;

/// <summary>
/// Metadata about a cache entry for monitoring and management
/// </summary>
public class CacheEntryMetadata
{
    /// <summary>
    /// The cache key
    /// </summary>
    public required string Key { get; set; }
    
    /// <summary>
    /// When the entry was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// When the entry was last accessed
    /// </summary>
    public DateTime LastAccessed { get; set; }
    
    /// <summary>
    /// When the entry becomes stale
    /// </summary>
    public DateTime? StaleAt { get; set; }
    
    /// <summary>
    /// When the entry expires
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Number of times the entry has been accessed
    /// </summary>
    public int AccessCount { get; set; }
    
    /// <summary>
    /// Size of the cached data in bytes
    /// </summary>
    public long SizeBytes { get; set; }
    
    /// <summary>
    /// Priority level for eviction
    /// </summary>
    public CacheEntryPriority Priority { get; set; }
    
    /// <summary>
    /// Tags associated with the entry
    /// </summary>
    public string[] Tags { get; set; } = [];
    
    /// <summary>
    /// Whether the entry is currently stale
    /// </summary>
    public bool IsStale => StaleAt.HasValue && DateTime.UtcNow >= StaleAt.Value;
    
    /// <summary>
    /// Whether the entry has expired
    /// </summary>
    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow >= ExpiresAt.Value;
}