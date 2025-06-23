using Invalid8.Core;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Invalid8.Providers.Base;

/// <summary>
/// Abstract base class for cache providers implementing both IBaseDistributedCache and ICacheProvider
/// Provides common functionality while allowing concrete providers to implement storage-specific logic
/// </summary>
public abstract class BaseCacheProvider(IKeyGenerator keyGenerator, ILogger<BaseCacheProvider> logger)
    : ICacheProvider
{
    protected readonly IKeyGenerator _keyGenerator = keyGenerator;
    protected readonly ILogger<BaseCacheProvider> _logger = logger;

    ~BaseCacheProvider()
    {
        Dispose(disposing: false);
    }

    // ----------------------------------------------------------------------------
    // IBaseDistributedCache Implementation (ABSTRACT - Must be implemented by derived)
    // ----------------------------------------------------------------------------

    public abstract byte[]? Get(string key);
    public abstract Task<byte[]?> GetAsync(string key, CancellationToken token = default);
    public abstract void Set(string key, byte[] value, DistributedCacheEntryOptions options);

    public abstract Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options,
        CancellationToken token = default);

    public abstract void Refresh(string key);
    public abstract Task RefreshAsync(string key, CancellationToken token = default);
    public abstract void Remove(string key);
    public abstract Task RemoveAsync(string key, CancellationToken token = default);

    // ----------------------------------------------------------------------------
    // ICacheProvider Implementation (VIRTUAL - Common logic with override capability)
    // ----------------------------------------------------------------------------

    public virtual async Task<CacheEntry<T>?> GetEntryAsync<T>(string[] key, CancellationToken ct = default)
    {
        var cacheKey = _keyGenerator.Generate(key);
        try
        {
            var data = await GetAsync(cacheKey, ct);
            if (data == null) return null;

            var entry = Deserialize<CacheEntry<T>>(data);
            entry.LastAccessed = DateTime.UtcNow;

            // Auto-refresh sliding expiration on access if configured
            if (entry.ExpiresAt.HasValue && HasSlidingExpiration(entry))
            {
                // For sliding expiration, we need to re-store with updated expiration
                await SetAsync(key, entry.Value, new CacheEntryOptions
                {
                    AbsoluteExpiration = entry.ExpiresAt,
                    SlidingExpiration = GetSlidingExpirationFromEntry(entry)
                }, ct);
            }

            return entry;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get cache entry for key {CacheKey}", cacheKey);
            throw;
        }
    }

    public virtual async Task SetAsync<T>(string[] key, T value, CacheEntryOptions options,
        CancellationToken ct = default)
    {
        var cacheKey = _keyGenerator.Generate(key);
        try
        {
            var entry = new CacheEntry<T>
            {
                Value = value,
                CreatedAt = DateTime.UtcNow,
                LastAccessed = DateTime.UtcNow,
                StaleAt = options.StaleTime.HasValue ? DateTime.UtcNow.Add(options.StaleTime.Value) : null,
                ExpiresAt = options.GetAbsoluteExpiration()?.UtcDateTime,
                Tags = options.Tags,
                Priority = options.Priority
            };

            var distributedOptions = ToDistributedCacheOptions(options);
            var serializedData = Serialize(entry);

            await SetAsync(cacheKey, serializedData, (DistributedCacheEntryOptions)distributedOptions, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set cache entry for key {CacheKey}", cacheKey);
            throw;
        }
    }

    public virtual async Task<bool> ExistsAsync(string[] key, CancellationToken ct = default)
    {
        var cacheKey = _keyGenerator.Generate(key);
        try
        {
            var data = await GetAsync(cacheKey, ct);
            return data != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check existence for key {CacheKey}", cacheKey);
            throw;
        }
    }

    public virtual async Task<CacheEntryMetadata?> GetMetadataAsync(string[] key, CancellationToken ct = default)
    {
        var cacheKey = _keyGenerator.Generate(key);
        try
        {
            var data = await GetAsync(cacheKey, ct);
            if (data == null) return null;

            // Read minimal metadata without deserializing the entire object
            using var stream = new MemoryStream(data);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

            return new CacheEntryMetadata
            {
                Key = cacheKey,
                CreatedAt = doc.RootElement.GetProperty("CreatedAt").GetDateTime(),
                LastAccessed = doc.RootElement.GetProperty("LastAccessed").GetDateTime(),
                StaleAt = doc.RootElement.TryGetProperty("StaleAt", out var staleAt) &&
                          staleAt.ValueKind != JsonValueKind.Null
                    ? staleAt.GetDateTime()
                    : null,
                ExpiresAt = doc.RootElement.TryGetProperty("ExpiresAt", out var expiresAt) &&
                            expiresAt.ValueKind != JsonValueKind.Null
                    ? expiresAt.GetDateTime()
                    : null,
                SizeBytes = data.LongLength
            };
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to get metadata for key {CacheKey}", cacheKey);
            return null;
        }
    }

    // ----------------------------------------------------------------------------
    // Helper Methods (PROTECTED - Available to derived classes)
    // ----------------------------------------------------------------------------

    protected virtual byte[] Serialize<T>(T value)
    {
        return JsonSerializer.SerializeToUtf8Bytes(value, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });
    }

    protected virtual T Deserialize<T>(byte[] data)
    {
        return JsonSerializer.Deserialize<T>(data, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }) ?? throw new InvalidOperationException("Failed to deserialize cache entry");
    }

    protected virtual DistributedCacheEntryOptions ToDistributedCacheOptions(CacheEntryOptions options)
    {
        var distributedOptions = new DistributedCacheEntryOptions();

        if (options.AbsoluteExpiration.HasValue)
        {
            distributedOptions.AbsoluteExpiration = options.AbsoluteExpiration;
        }

        if (options.AbsoluteExpirationRelativeToNow.HasValue)
        {
            distributedOptions.AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow;
        }

        if (options.SlidingExpiration.HasValue)
        {
            distributedOptions.SlidingExpiration = options.SlidingExpiration;
        }

        return distributedOptions;
    }

    protected virtual bool HasSlidingExpiration<T>(CacheEntry<T> entry)
    {
        // Check if this entry should have sliding expiration behavior
        // This is custom logic since .NET doesn't store sliding expiration in the cached data
        return entry.ExpiresAt.HasValue && entry.CreatedAt != entry.LastAccessed;
    }

    protected virtual TimeSpan? GetSlidingExpirationFromEntry<T>(CacheEntry<T> entry)
    {
        // Calculate what the original sliding expiration might have been
        if (!entry.ExpiresAt.HasValue || entry.CreatedAt == entry.LastAccessed)
            return null;

        return entry.ExpiresAt.Value - entry.LastAccessed;
    }

    // ----------------------------------------------------------------------------
    // Bulk Operations (VIRTUAL - Optional implementation)
    // ----------------------------------------------------------------------------

    public virtual async Task<IDictionary<string[], T>> GetBulkAsync<T>(IEnumerable<string[]> keys,
        CancellationToken ct = default)
    {
        var results = new Dictionary<string[], T>();
        foreach (var key in keys)
        {
            var entry = await GetEntryAsync<T>(key, ct);
            if (entry != null)
            {
                results[key] = entry.Value;
            }
        }

        return results;
    }

    public virtual async Task SetBulkAsync<T>(IDictionary<string[], T> values, CacheEntryOptions options,
        CancellationToken ct = default)
    {
        foreach (var (key, value) in values)
        {
            await SetAsync(key, value, options, ct);
        }
    }

    public virtual async Task RemoveBulkAsync(IEnumerable<string[]> keys, CancellationToken ct = default)
    {
        foreach (var key in keys)
        {
            var cacheKey = _keyGenerator.Generate(key);
            await RemoveAsync(cacheKey, ct);
        }
    }

    // ----------------------------------------------------------------------------
    // Tag-based Invalidation (VIRTUAL - Optional implementation)
    // ----------------------------------------------------------------------------

    public virtual Task InvalidateByTagAsync(string tag, CancellationToken ct = default)
    {
        // Base implementation does nothing - concrete providers should override
        // if they support tag-based invalidation natively
        _logger.LogDebug("Tag-based invalidation not implemented for this provider");
        return Task.CompletedTask;
    }

    // ----------------------------------------------------------------------------
    // Dispose Pattern (VIRTUAL - Cleanup resources)
    // ----------------------------------------------------------------------------

    protected virtual void Dispose(bool disposing)
    {
        // Cleanup resources if needed
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public ValueTask DisposeAsync()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}