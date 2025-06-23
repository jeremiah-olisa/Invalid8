using Invalid8.Core;
using Invalid8.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace Invalid8;

/// <summary>
/// Main implementation of IQueryClient providing React Query-like developer experience
/// with comprehensive caching, mutation, and invalidation support
/// </summary>
public sealed class QueryClient : IQueryClient
{
    private readonly ICacheProvider _cacheProvider;
    private readonly IEventProvider? _eventProvider;
    private readonly IKeyGenerator _keyGenerator;
    private readonly ILogger<QueryClient> _logger;
    private readonly Dictionary<string, QueryMetadata> _queryMetadata = new();

    public QueryClient(
        ICacheProvider cacheProvider,
        IEventProvider? eventProvider,
        IKeyGenerator keyGenerator,
        ILogger<QueryClient> logger)
    {
        _cacheProvider = cacheProvider;
        _eventProvider = eventProvider;
        _keyGenerator = keyGenerator;
        _logger = logger;

        // Subscribe to invalidation events
        _ = SubscribeToInvalidationEvents();
    }

    ~QueryClient()  // Finalizer
    {
        Dispose(disposing: false);
    }

    public QueryClient(
        ICacheProvider cacheProvider,
        IKeyGenerator keyGenerator,
        ILogger<QueryClient> logger)
    {
        _cacheProvider = cacheProvider;
        _keyGenerator = keyGenerator;
        _logger = logger;
    }

    // ----------------------------------------------------------------------------
    // QUERY OPERATIONS
    // ----------------------------------------------------------------------------

    public async Task<QueryResult<T>> QueryAsync<T>(
        string[] key,
        Func<Task<T>> queryFunc,
        QueryOptions? options = null,
        CancellationToken ct = default)
    {
        var cacheKey = _keyGenerator.Generate(key);
        options ??= new QueryOptions();

        try
        {
            // Try to get from cache first
            var cachedEntry = await _cacheProvider.GetEntryAsync<T>(key, ct);
            if (cachedEntry is { IsExpired: false })
            {
                _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
                UpdateQueryMetadata(key, true, true);
                return QueryResult<T>.FromCache(cachedEntry.Value, cachedEntry.CreatedAt,
                    cachedEntry.StaleAt ?? DateTime.UtcNow, cachedEntry.ExpiresAt ?? DateTime.UtcNow);
            }

            _logger.LogDebug("Cache miss for key: {CacheKey}", cacheKey);

            // Execute query
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var data = await queryFunc();
            sw.Stop();

            // Cache the result
            var cacheOptions = new CacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = options.CacheTime,
                SlidingExpiration = options.StaleTime.HasValue
                    ? TimeSpan.FromTicks(options.StaleTime.Value.Ticks * 2)
                    : null
            };

            await _cacheProvider.SetAsync(key, data, cacheOptions, ct);

            UpdateQueryMetadata(key, false, true, sw.ElapsedMilliseconds);

            return QueryResult<T>.FromFresh(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Query failed for key: {CacheKey}", cacheKey);
            UpdateQueryMetadata(key, false, false, error: ex);
            return QueryResult<T>.FromError(ex);
        }
    }

    public async Task<QueryResult<T>?> GetQueryDataAsync<T>(string[] key, CancellationToken ct = default)
    {
        try
        {
            var cachedEntry = await _cacheProvider.GetEntryAsync<T>(key, ct);
            if (cachedEntry == null) return null;

            UpdateQueryMetadata(key, true, true);
            return QueryResult<T>.FromCache(cachedEntry.Value, cachedEntry.CreatedAt,
                cachedEntry.StaleAt ?? DateTime.UtcNow, cachedEntry.ExpiresAt ?? DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get query data for key: {Key}", _keyGenerator.Generate(key));
            return null;
        }
    }

    public async Task SetQueryDataAsync<T>(
        string[] key,
        T data,
        CacheEntryOptions? options = null,
        CancellationToken ct = default)
    {
        options ??= new CacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        };

        await _cacheProvider.SetAsync(key, data, options, ct);
        _logger.LogDebug("Manually set query data for key: {Key}", _keyGenerator.Generate(key));
    }

    public async Task UpdateQueryDataAsync<T>(
        string[] key,
        Func<T?, T> updateFunction,
        CacheEntryOptions? options = null,
        CancellationToken ct = default)
    {
        var current = await GetQueryDataAsync<T>(key, ct);

        if (current == null) return;
        // Extract the current data or use default
        var updated = updateFunction(current.Data);

        await SetQueryDataAsync(key, updated, options, ct);


        _logger.LogDebug("Updated query data for key: {Key}", _keyGenerator.Generate(key));
    }

    // ----------------------------------------------------------------------------
    // INVALIDATION & REFETCHING
    // ----------------------------------------------------------------------------

    public async Task InvalidateQueriesAsync(
        string[]? keyPrefix = null,
        bool invalidateDescendants = false,
        CancellationToken ct = default)
    {
        if (keyPrefix == null)
        {
            // Invalidate all queries
            await _cacheProvider.RemoveBulkAsync(GetAllQueryKeys(), ct);
            _logger.LogInformation("Invalidated all queries");
            return;
        }


        // Generate the cache key from the key parts
        var cacheKey = _keyGenerator.Generate(keyPrefix);

        // TODO: Implement pattern-based invalidation
        // For now, invalidate exact match
        await _cacheProvider.RemoveAsync(cacheKey, ct);
        _logger.LogDebug("Invalidated query: {Key}", _keyGenerator.Generate(keyPrefix));
    }

    public async Task RefetchQueriesAsync(
        string[]? keyPrefix = null,
        bool refetchDescendants = false,
        CancellationToken ct = default)
    {
        // Refetch is essentially invalidation + trigger re-fetch
        await InvalidateQueriesAsync(keyPrefix, refetchDescendants, ct);
        _logger.LogDebug("Refetched queries for prefix: {Prefix}",
            keyPrefix != null ? _keyGenerator.Generate(keyPrefix) : "all");
    }

    // ----------------------------------------------------------------------------
    // MUTATION OPERATIONS
    // ----------------------------------------------------------------------------

    public async Task<T> MutateAsync<T>(
        Func<Task<T>> mutationFunc,
        MutationOptions? options = null,
        CancellationToken ct = default)
    {
        options ??= new MutationOptions();

        try
        {
            var result = await mutationFunc();

            // Invalidate related queries after successful mutation
            if (options.InvalidateQueries != null)
            {
                await InvalidateRelatedQueries(options.InvalidateQueries, ct);
            }

            _logger.LogDebug("Mutation completed successfully");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mutation failed");
            throw;
        }
    }

    public async Task<T> MutateAsync<T>(
        Func<Task<T>> mutationFunc,
        OptimisticMutationOptions<T> options,
        CancellationToken ct = default)
    {
        try
        {
            // Apply optimistic update
            foreach (var key in options.QueryKeys)
            {
                // Explicitly specify the type parameter
                await UpdateQueryDataAsync<T>(key,
                    current =>
                    {
                        var optimisticData = options.OptimisticData();
                        return optimisticData ?? current ??
                            throw new InvalidOperationException("Optimistic data and current data cannot both be null");
                    },
                    null, ct);
            }

            // Execute mutation
            var result = await mutationFunc();

            // Apply success update if provided
            if (options.OnSuccess != null)
            {
                foreach (var key in options.QueryKeys)
                {
                    // Explicitly specify the type parameter for the lambda
                    // Only update if we have current data to avoid overwriting
                    // potentially newer data from other operations
                    await UpdateQueryDataAsync<T>(key, current => current != null ? options.OnSuccess!(result) : result,
                        null, ct);
                }
            }
            else
            {
                // If no OnSuccess provided, just update with the mutation result
                foreach (var key in options.QueryKeys)
                {
                    await SetQueryDataAsync(key, result, null, ct);
                }
            }

            _logger.LogDebug("Optimistic mutation completed successfully");
            return result;
        }
        catch (Exception ex)
        {
            // Rollback on error
            if (options.RollbackOnError)
            {
                _logger.LogWarning(ex, "Mutation failed, rolling back optimistic updates");
                foreach (var key in options.QueryKeys)
                {
                    await InvalidateQueriesAsync(key, false, ct);
                }
            }

            options.OnError?.Invoke(ex);

            // Re-throw to maintain error behavior
            throw new MutationException("Optimistic mutation failed", ex);
        }
    }

    // ----------------------------------------------------------------------------
    // METADATA & MANAGEMENT
    // ----------------------------------------------------------------------------
    public Task<QueryMetadata?> GetQueryMetadataAsync(string[] key, CancellationToken ct = default)
    {
        var cacheKey = _keyGenerator.Generate(key);
        _queryMetadata.TryGetValue(cacheKey, out var metadata);
        return Task.FromResult(metadata);
    }

    public Task<IReadOnlyList<string[]>> GetQueryKeysAsync(CancellationToken ct = default)
    {
        // TODO: Implement proper query key tracking
        // For now, return empty list - this would require maintaining state
        return Task.FromResult<IReadOnlyList<string[]>>([]);
    }

    public async Task ResetAsync(CancellationToken ct = default)
    {
        await _cacheProvider.RemoveBulkAsync(GetAllQueryKeys(), ct);
        _queryMetadata.Clear();
        _logger.LogInformation("Query client reset complete");
    }

    // ----------------------------------------------------------------------------
    // PRIVATE HELPER METHODS
    // ----------------------------------------------------------------------------

    private async Task SubscribeToInvalidationEvents()
    {
        if (_eventProvider is null) return;

        await _eventProvider.SubscribeToInvalidationsAsync(async invalidationEvent =>
        {
            ArgumentNullException.ThrowIfNull(invalidationEvent);
            ArgumentNullException.ThrowIfNull(invalidationEvent.Keys);
            foreach (var key in invalidationEvent.Keys)
            {
                var queryKey = _keyGenerator.Generate(key);
                await _cacheProvider.RemoveAsync(queryKey, CancellationToken.None);
                _logger.LogDebug("Invalidated via event: {Key}", _keyGenerator.Generate(key));
            }
        });
    }

    private void UpdateQueryMetadata(string[] key, bool isCacheHit, bool isSuccess,
        long durationMs = 0, Exception? error = null)
    {
        var cacheKey = _keyGenerator.Generate(key);

        if (!_queryMetadata.TryGetValue(cacheKey, out var metadata))
        {
            metadata = new QueryMetadata { Key = key };
            _queryMetadata[cacheKey] = metadata;
        }

        metadata.LastFetched = DateTime.UtcNow;
        metadata.FetchCount++;

        if (isCacheHit) metadata.CacheHitCount++;
        if (isSuccess)
            metadata.AverageFetchDurationMs =
                (metadata.AverageFetchDurationMs * (metadata.FetchCount - 1) + durationMs) / metadata.FetchCount;

        if (error == null) return;

        metadata.LastError = error;
        metadata.LastErrorAt = DateTime.UtcNow;
    }

    private async Task InvalidateRelatedQueries(string[][] keysToInvalidate, CancellationToken ct)
    {
        foreach (var key in keysToInvalidate)
        {
            var queryKey = _keyGenerator.Generate(key);
            await _cacheProvider.RemoveAsync(queryKey, ct);

            if (_eventProvider is null) return;
            // Publish invalidation event for distributed scenarios
            await _eventProvider.PublishInvalidationAsync(new CacheInvalidationEvent
            {
                Keys = [key],
                Reason = InvalidationReason.Manual,
                SourceInstance = Environment.MachineName,
                Timestamp = DateTime.UtcNow
            }, ct);
        }
    }

    private IEnumerable<string[]> GetAllQueryKeys()
    {
        // TODO: Implement proper query key tracking
        // For now, return empty - this would require maintaining state
        return [];
    }

    // ----------------------------------------------------------------------------
    // DISPOSABLE PATTERN
    // ----------------------------------------------------------------------------

    private bool _disposed;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Cleanup managed resources
                _queryMetadata.Clear();
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    private ValueTask DisposeAsyncCore()
    {
        // Async cleanup if needed
        _queryMetadata.Clear();
        return ValueTask.CompletedTask;
    }
}