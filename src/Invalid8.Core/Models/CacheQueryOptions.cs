namespace Invalid8.Core;

public class CacheQueryOptions
{
    public TimeSpan? StaleTime { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan? CacheTime { get; set; } = TimeSpan.FromHours(1);
    public int RetryCount { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
    public bool EnableBackgroundRefetch { get; set; } = true;
}

