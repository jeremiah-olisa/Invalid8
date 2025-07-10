using Invalid8.Core;
using Microsoft.Extensions.Caching.Memory;

namespace Invalid8.InMemory;

public class MemoryCacheProvider : ICacheProvider
{
    private readonly IMemoryCache _cache;
    public Guid InstanceId { get; } = Guid.NewGuid();

    public MemoryCacheProvider(IMemoryCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }


    public Task<T?> GetAsync<T>(string[] key, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (key.Length == 0) throw new ArgumentException("Key cannot be empty", nameof(key));

        var cacheKey = GenerateCacheKey(key);
        ct.ThrowIfCancellationRequested();

        if (_cache.TryGetValue(cacheKey, out var value) && value is T typedValue)
        {
            return Task.FromResult<T?>(typedValue);
        }

        return Task.FromResult<T?>(default);
    }

    public Task SetAsync<T>(string[] key, T value, CacheQueryOptions options, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(options);
        if (key.Length == 0) throw new ArgumentException("Key cannot be empty", nameof(key));

        var cacheKey = GenerateCacheKey(key);
        ct.ThrowIfCancellationRequested();

        var cacheEntryOptions = new MemoryCacheEntryOptions();

        // Set TTL based on your CacheQueryOptions
        if (options.CacheTime.HasValue)
        {
            cacheEntryOptions.AbsoluteExpirationRelativeToNow = options.CacheTime.Value;
        }

        if (options.StaleTime.HasValue)
        {
            // Use sliding expiration for stale time
            cacheEntryOptions.SlidingExpiration = options.StaleTime.Value;
        }

        // Set cache priority if needed
        cacheEntryOptions.Priority = CacheItemPriority.Normal;

        _cache.Set(cacheKey, value, cacheEntryOptions);
        return Task.CompletedTask;
    }

    public Task<CacheEntryMetadata?> GetEntryMetadataAsync(string[] key, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (key.Length == 0) throw new ArgumentException("Key cannot be empty", nameof(key));

        string cacheKey = GenerateCacheKey(key);
        ct.ThrowIfCancellationRequested();

        // MemoryCache doesn't expose metadata directly, so we'll do a simple check
        if (_cache.TryGetValue(cacheKey, out _))
        {
            return Task.FromResult<CacheEntryMetadata?>(new CacheEntryMetadata
            {
                IsStale = false, // Can't determine without additional tracking
                IsExpired = false // If we can get it, it's not expired
            });
        }

        return Task.FromResult<CacheEntryMetadata?>(null);
    }

    private static string GenerateCacheKey(string[] key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (key.Length == 0 || key.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("Key cannot be empty or contain whitespace", nameof(key));

        return string.Join(":", key);
    }

    public Task InvalidateAsync(string[] key, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (key.Length == 0) throw new ArgumentException("Key cannot be empty", nameof(key));

        var cacheKey = GenerateCacheKey(key);
        ct.ThrowIfCancellationRequested();

        _cache.Remove(cacheKey);
        return Task.CompletedTask;
    }

    public Task InvalidateByPatternAsync(string pattern, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        if (string.IsNullOrWhiteSpace(pattern))
            throw new ArgumentException("Pattern cannot be empty or whitespace", nameof(pattern));

        // Note: MemoryCache doesn't expose keys directly for pattern matching
        // This is a limitation of the built-in MemoryCache
        // You would need to maintain a separate key registry or use a different approach

        throw new NotSupportedException(
            "Pattern-based invalidation is not supported with MemoryCache. " +
            "Consider using a different cache provider or maintaining a key registry.");
    }
}
