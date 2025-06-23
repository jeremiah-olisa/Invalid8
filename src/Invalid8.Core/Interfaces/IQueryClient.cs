namespace Invalid8.Core;

/// <summary>
/// Main client for executing queries and mutations with caching (React Query-like DX)
/// </summary>
public interface IQueryClient : IAsyncDisposable
{
    // QUERY OPERATIONS
    /// <summary>
    /// Executes a query with caching support
    /// </summary>
    Task<QueryResult<T>> QueryAsync<T>(
        string[] key,
        Func<Task<T>> queryFunc,
        QueryOptions? options = null,
        CancellationToken ct = default);
    
    /// <summary>
    /// Gets a query result without executing the query function (returns null if not cached)
    /// </summary>
    Task<QueryResult<T>?> GetQueryDataAsync<T>(string[] key, CancellationToken ct = default);
    
    /// <summary>
    /// Manually sets query data in the cache
    /// </summary>
    Task SetQueryDataAsync<T>(
        string[] key,
        T data,
        CacheEntryOptions? options = null,
        CancellationToken ct = default);
    
    /// <summary>
    /// Updates query data using a transformation function
    /// </summary>
    Task UpdateQueryDataAsync<T>(
        string[] key,
        Func<T?, T> updateFunction,
        CacheEntryOptions? options = null,
        CancellationToken ct = default);
    
    /// <summary>
    /// Manually invalidates cache entries
    /// </summary>
    Task InvalidateQueriesAsync(
        string[]? keyPrefix = null, 
        bool invalidateDescendants = false,
        CancellationToken ct = default);
    
    /// <summary>
    /// Manually refreshes cache entries
    /// </summary>
    Task RefetchQueriesAsync(
        string[]? keyPrefix = null,
        bool refetchDescendants = false,
        CancellationToken ct = default);
    
    // MUTATION OPERATIONS (NO KEY NEEDED)
    /// <summary>
    /// Executes a mutation with automatic cache invalidation support
    /// </summary>
    Task<T> MutateAsync<T>(
        Func<Task<T>> mutationFunc,
        MutationOptions? options = null,
        CancellationToken ct = default);
    
    /// <summary>
    /// Executes a mutation with optimistic update support
    /// </summary>
    Task<T> MutateAsync<T>(
        Func<Task<T>> mutationFunc,
        OptimisticMutationOptions<T> options,
        CancellationToken ct = default);
    
    
    
    // METADATA & MANAGEMENT
    /// <summary>
    /// Gets metadata about a cached query
    /// </summary>
    Task<QueryMetadata?> GetQueryMetadataAsync(string[] key, CancellationToken ct = default);
    
    /// <summary>
    /// Gets all active query keys
    /// </summary>
    Task<IReadOnlyList<string[]>> GetQueryKeysAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Resets the query client (clears all cache and state)
    /// </summary>
    Task ResetAsync(CancellationToken ct = default);
}