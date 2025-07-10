namespace Invalid8.Core;

public class CacheInvalidationEvent
{
    public string[] Key { get; set; }
    public DateTimeOffset Timestamp { get; set; }

    public CacheInvalidationEvent(string[] key)
    {
        Key = key;
        Timestamp = DateTimeOffset.UtcNow;
    }
}
