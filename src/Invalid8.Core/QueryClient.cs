using Invalid8.Core.Models;
using Microsoft.Extensions.Logging;

namespace Invalid8.Core;

public class QueryClient : IQueryClient
{
    private readonly IEnumerable<ICacheProvider> _cacheProviders;
    private readonly IEnumerable<IEventProvider> _eventProviders;
    private readonly IGenerateKey _keyGenerator;
    private readonly ILogger<QueryClient> _logger;
    private readonly ICacheProvider _defaultCacheProvider;
    private readonly IEventProvider _defaultEventProvider;

    public QueryClient(
        IEnumerable<ICacheProvider> cacheProviders,
        IEnumerable<IEventProvider> eventProviders,
        IGenerateKey keyGenerator,
        ILogger<QueryClient> logger)
    {
        _cacheProviders = cacheProviders ?? throw new ArgumentNullException(nameof(cacheProviders));
        _eventProviders = eventProviders ?? throw new ArgumentNullException(nameof(eventProviders));
        _keyGenerator = keyGenerator ?? throw new ArgumentNullException(nameof(keyGenerator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (!_cacheProviders.Any())
            throw new ArgumentException("At least one cache provider must be registered.", nameof(cacheProviders));
        if (!_eventProviders.Any())
            throw new ArgumentException("At least one event provider must be registered.", nameof(eventProviders));

        _defaultCacheProvider = _cacheProviders.First();
        _defaultEventProvider = _eventProviders.First();
    }

    public async Task<QueryResult<T>> UseCachedQueryAsync<T>(
        string[] key,
        Func<Task<T>> queryMethod,
        CacheQueryOptions? options = default,
        string? cacheProviderName = null,
        string? eventProviderName = null,
        CancellationToken ct = default)
    {
        ICacheProvider cacheProvider = GetCacheProvider(cacheProviderName);
        IEventProvider eventProvider = GetEventProvider(eventProviderName);
        CacheEntryMetadata? metadata = await cacheProvider.GetEntryMetadataAsync(key, ct);
        T? cacheEntry = await cacheProvider.GetAsync<T>(key, ct);

        options ??= new();

        if (cacheEntry != null && !metadata!.IsExpired)
        {
            if (!metadata.IsStale || !options.EnableBackgroundRefetch)
            {
                return new QueryResult<T>(cacheEntry, true, metadata.IsStale);
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    var freshData = await queryMethod();
                    await cacheProvider.SetAsync(key, freshData, options, ct);
                    await eventProvider.PublishAsync(new CacheUpdatedEvent(key, freshData), ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Background refresh failed for cache key {CacheKey} with provider {ProviderName}",
                        key, cacheProvider.GetType().Name);
                }
            }, ct);

            return new QueryResult<T>(cacheEntry, true, metadata.IsStale);
        }

        var data = await queryMethod();

        await cacheProvider.SetAsync(key, data, options, ct);
        await eventProvider.PublishAsync(new CacheUpdatedEvent(key, data), ct);

        return new QueryResult<T>(data, false, false);
    }

    public async Task<T> UseMutateQueryAsync<T>(
        string[] key,
        Func<Task<T>> mutationFunc,
        MutationOptions? options = default,
        string? cacheProviderName = null,
        string? eventProviderName = null,
        CancellationToken ct = default)
    {
        var cacheProvider = GetCacheProvider(cacheProviderName);
        var eventProvider = GetEventProvider(eventProviderName);

        var result = await mutationFunc();

        // Combine input key with InvalidationKeys, ensuring uniqueness
        List<string[]> invalidationKeys = [key];
        if (options?.InvalidationKeys != null)
            invalidationKeys.AddRange(options.InvalidationKeys);


        // Remove duplicates based on composite key
        var uniqueKeys = invalidationKeys
            .GroupBy(k => _keyGenerator.Generate(k))
            .Select(g => g.First())
            .ToList();

        foreach (var invalidationKey in uniqueKeys)
        {
            await cacheProvider.InvalidateAsync(invalidationKey, ct);
            await eventProvider.PublishAsync(new CacheInvalidationEvent(invalidationKey), ct);
        }

        return result;
    }

    private ICacheProvider GetCacheProvider(string? providerName)
    {
        if (string.IsNullOrEmpty(providerName))
            return _defaultCacheProvider;

        var provider = _cacheProviders.FirstOrDefault(p => p.GetType().Name == providerName)
            ?? throw new ArgumentException($"No cache provider found with name: {providerName}");
        return provider;
    }

    private IEventProvider GetEventProvider(string? providerName)
    {
        if (string.IsNullOrEmpty(providerName))
            return _defaultEventProvider;

        var provider = _eventProviders.FirstOrDefault(p => p.GetType().Name == providerName)
            ?? throw new ArgumentException($"No event provider found with name: {providerName}");
        return provider;
    }
}