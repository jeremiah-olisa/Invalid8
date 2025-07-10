namespace Invalid8.Core;

public class CacheEntry<T>
{
    public T? Data { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastAccessedAt { get; set; }
    public TimeSpan? StaleTime { get; set; }
    public TimeSpan? CacheTime { get; set; }

    public bool IsStale => StaleTime.HasValue &&
        DateTime.UtcNow - CreatedAt > StaleTime.Value;

    public bool IsExpired => CacheTime.HasValue &&
        DateTime.UtcNow - CreatedAt > CacheTime.Value;
}

public class CacheEntryMetadata
{
    public bool IsStale { get; set; }
    public bool IsExpired { get; set; }
}