namespace Invalid8.Core;

public interface IQueryClient
{
    Task<QueryResult<T>> UseCachedQueryAsync<T>(
        string[] key,
        Func<Task<T>> queryMethod,
        CacheQueryOptions? options = default,
        string? cacheProviderName = null,
        string? eventProviderName = null,
        CancellationToken ct = default);

    Task<T> UseMutateQueryAsync<T>(
        string[] key,
        Func<Task<T>> mutationFunc,
        MutationOptions? options = default,
        string? cacheProviderName = null,
        string? eventProviderName = null,
        CancellationToken ct = default);
}