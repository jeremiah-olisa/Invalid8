namespace Invalid8.Core;

public class CacheInvalidationEvent(string[] key)
{
    public string[] Key { get; set; } = key;
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
