using Invalid8.Core.Models;
using Microsoft.Extensions.Logging;

namespace Invalid8.Core;

public class QueryClient : IQueryClient
{
    private readonly ICacheProvider _cache;
    private readonly IEventProvider _eventProvider;
    private readonly IGenerateKey _keyGenerator;
    private readonly ILogger<QueryClient> _logger;

    public QueryClient(ICacheProvider cache, IEventProvider eventProvider, IGenerateKey keyGenerator, ILogger<QueryClient> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _eventProvider = eventProvider ?? throw new ArgumentNullException(nameof(eventProvider));
        _keyGenerator = keyGenerator ?? throw new ArgumentNullException(nameof(keyGenerator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<QueryResult<T>> UseCachedQueryAsync<T>(
        string[] key,
        Func<Task<T>> queryMethod,
        CacheQueryOptions options,
        CancellationToken ct = default)
    {
        var cacheKey = _keyGenerator.Generate(key);
        var metadata = await _cache.GetEntryMetadataAsync(key, ct);
        var cacheEntry = await _cache.GetAsync<T>(key, ct);

        if (cacheEntry != null && !metadata!.IsExpired)
        {
            if (!metadata.IsStale || !options.EnableBackgroundRefetch)
            {
                return new QueryResult<T> { Data = cacheEntry, IsFromCache = true, IsStale = metadata.IsStale };
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    var freshData = await queryMethod();
                    await _cache.SetAsync(key, freshData, options, ct);
                    await _eventProvider.PublishAsync(new CacheUpdatedEvent(key, freshData), ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Background refresh failed for cache key {CacheKey}", cacheKey);
                }
            }, ct);

            return new QueryResult<T> { Data = cacheEntry, IsFromCache = true, IsStale = metadata.IsStale };
        }

        var data = await queryMethod();
        await _cache.SetAsync(key, data, options, ct);
        await _eventProvider.PublishAsync(new CacheUpdatedEvent(key, data), ct);
        return new QueryResult<T> { Data = data, IsFromCache = false, IsStale = false };
    }

    public async Task<T> UseMutateQueryAsync<T>(
        string[] key,
        Func<Task<T>> mutationFunc,
        MutationOptions options,
        CancellationToken ct = default)
    {
        var result = await mutationFunc();
        await _cache.InvalidateAsync(key, ct);
        await _eventProvider.PublishAsync(new CacheInvalidationEvent(key), ct);

        if (options?.InvalidationKeys != null)
        {
            foreach (var invalidationKey in options.InvalidationKeys)
            {
                await _cache.InvalidateAsync(invalidationKey, ct);
                await _eventProvider.PublishAsync(new CacheInvalidationEvent(invalidationKey), ct);
            }
        }

        return result;
    }
}