namespace Invalid8.Core.Models;

public class CacheUpdatedEvent(string[] key, object? data)
{
    public string[] Key { get; init; } = key ?? throw new ArgumentNullException(nameof(key));
    public object? Data { get; init; } = data;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}