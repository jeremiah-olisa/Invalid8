using Invalid8.Core;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Invalid8.InMemory;

public class InMemoryCacheProvider : ICacheProvider
{
    private readonly ConcurrentDictionary<string, CacheEntry<object>> _cache = new();

    public async Task<T?> GetAsync<T>(string[] key, CancellationToken ct = default)
    {
        var cacheKey = GenerateCacheKey(key, ct);

        // Early return if cancellation is requested
        ct.ThrowIfCancellationRequested();

        if (_cache.TryGetValue(cacheKey, out var entry) && entry is CacheEntry<T> typedEntry)
        {
            if (typedEntry.IsExpired)
            {
                // Use TryRemoveAsync if available, or keep synchronous version
                _cache.TryRemove(cacheKey, out _);
                return default;
            }

            // Update last accessed time asynchronously if needed
            typedEntry.LastAccessedAt = DateTime.UtcNow;

            // If data might need async loading, you could await here
            return typedEntry.Data;
        }

        // Return completed task with default value
        return await Task.FromResult<T?>(default);
    }

    public Task SetAsync<T>(string[] key, T value, CacheQueryOptions options, CancellationToken ct = default)
    {
        var cacheKey = GenerateCacheKey(key, ct);

        var entry = new CacheEntry<object>
        {
            Data = value,
            CreatedAt = DateTime.UtcNow,
            LastAccessedAt = DateTime.UtcNow,
            CacheTime = options.CacheTime,
            StaleTime = options.StaleTime
        };
        _cache[cacheKey] = entry;

        return Task.CompletedTask;
    }

    public Task<CacheEntryMetadata?> GetEntryMetadataAsync(string[] key, CancellationToken ct = default)
    {
        string cacheKey = GenerateCacheKey(key, ct);

        if (_cache.TryGetValue(cacheKey, out var entry))
        {
            return Task.FromResult<CacheEntryMetadata?>(new CacheEntryMetadata
            {
                IsStale = entry.IsStale,
                IsExpired = entry.IsExpired
            });
        }

        return Task.FromResult<CacheEntryMetadata?>(null);
    }

    private static string GenerateCacheKey(string[] key, CancellationToken ct = default)
    {
        // Validate input
        ArgumentNullException.ThrowIfNull(key);

        if (key.Length == 0 || key.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("Key cannot be empty or contain whitespace", nameof(key));

        // Early cancellation check
        ct.ThrowIfCancellationRequested();

        var cacheKey = string.Join(":", key);
        return cacheKey;
    }

    public Task InvalidateAsync(string[] key, CancellationToken ct = default)
    {
        var cacheKey = GenerateCacheKey(key, ct);

        _cache.TryRemove(cacheKey, out _);

        return Task.CompletedTask;
    }

    public Task InvalidateByPatternAsync(string pattern, CancellationToken ct = default)
    {
        // Convert simple wildcards to regex (optional)
        var regexPattern = pattern
            .Replace(".", "\\.")  // Escape dots
            .Replace("*", ".*")   // Convert * to .*
            .Replace("?", ".");   // Convert ? to .

        var regex = new Regex(regexPattern, RegexOptions.Compiled);

        // Get all keys matching the pattern
        var keysToRemove = _cache.Keys
            .Where(k => regex.IsMatch(k))
            .ToList();

        // Remove all matching keys
        foreach (var key in keysToRemove)
        {
            ct.ThrowIfCancellationRequested();
            _cache.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }
}