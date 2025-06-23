using Microsoft.Extensions.Caching.Distributed;

namespace Invalid8.Core;

/// <summary>
/// Enhanced distributed cache provider with additional capabilities beyond IDistributedCache
/// </summary>
public interface ICacheProvider : IDistributedCache, IAsyncDisposable
{
    /// <summary>
    /// Gets a value from the cache with metadata
    /// </summary>
    Task<CacheEntry<T>?> GetEntryAsync<T>(string[] key, CancellationToken ct = default);

    /// <summary>
    /// Sets a value in the cache with extended options
    /// </summary>
    Task SetAsync<T>(string[] key, T value, CacheEntryOptions options, CancellationToken ct = default);

    /// <summary>
    /// Checks if a key exists in the cache
    /// </summary>
    Task<bool> ExistsAsync(string[] key, CancellationToken ct = default);

    /// <summary>
    /// Gets metadata about a cache entry without retrieving the value
    /// </summary>
    Task<CacheEntryMetadata?> GetMetadataAsync(string[] key, CancellationToken ct = default);

    /// <summary>
    /// Gets multiple values in a single operation
    /// </summary>
    Task<IDictionary<string[], T>> GetBulkAsync<T>(IEnumerable<string[]> keys, CancellationToken ct = default);

    /// <summary>
    /// Sets multiple values in a single operation
    /// </summary>
    Task SetBulkAsync<T>(IDictionary<string[], T> values, CacheEntryOptions options, CancellationToken ct = default);

    /// <summary>
    /// Removes multiple keys in a single operation
    /// </summary>
    Task RemoveBulkAsync(IEnumerable<string[]> keys, CancellationToken ct = default);

    /// <summary>
    /// Invalidates cache entries by tag pattern
    /// </summary>
    Task InvalidateByTagAsync(string tag, CancellationToken ct = default);
}