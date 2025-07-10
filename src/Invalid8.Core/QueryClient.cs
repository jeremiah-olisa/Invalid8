using Microsoft.Extensions.Logging;

namespace Invalid8.Core;

public class QueryClient : IQueryClient
{
    private readonly ICacheProvider _cacheProvider;
    private readonly IEventProvider _eventProvider;
    private readonly ILogger<QueryClient> _logger;

    public QueryClient(
        ICacheProvider cacheProvider,
        IEventProvider eventProvider,
        ILogger<QueryClient> logger)
    {
        _cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));
        _eventProvider = eventProvider ?? throw new ArgumentNullException(nameof(eventProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<QueryResult<T>> UseCachedQueryAsync<T>(
        string[] key,
        Func<Task<T>> queryMethod,
        CacheQueryOptions? options = null,
        CancellationToken ct = default)
    {
        options ??= new CacheQueryOptions();
        var result = new QueryResult<T>();
        var cacheKey = string.Join(":", key);
        var cached = await _cacheProvider.GetAsync<T>(key, ct);

        if (cached != null)
        {
            result.Data = cached;
            result.IsFromCache = true;

            var metadata = await _cacheProvider.GetEntryMetadataAsync(key, ct);
            result.IsStale = metadata?.IsStale ?? false;

            if (options.EnableBackgroundRefetch && result.IsStale)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var freshData = await queryMethod();
                        await _cacheProvider.SetAsync(key, freshData, options, ct);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Background refresh failed for cache key {CacheKey}", cacheKey);
                    }
                }, ct);
            }
            return result;
        }

        result.Data = await queryMethod();
        await _cacheProvider.SetAsync(key, result.Data, options, ct);
        return result;
    }

    public async Task<T> UseMutateQueryAsync<T>(
        string[] key,
        Func<Task<T>> mutationFunc,
        MutationOptions? options = null,
        CancellationToken ct = default)
    {
        options ??= new MutationOptions();
        var result = await mutationFunc();

        if (options.InvalidateQueries)
        {
            await _cacheProvider.InvalidateAsync(key, ct);
            await _eventProvider.PublishAsync(new CacheInvalidationEvent(key), ct);

            foreach (var invalidationKey in options.InvalidationKeys)
            {
                await _cacheProvider.InvalidateAsync(invalidationKey, ct);
                await _eventProvider.PublishAsync(new CacheInvalidationEvent(invalidationKey), ct);
            }
        }

        return result;
    }
}