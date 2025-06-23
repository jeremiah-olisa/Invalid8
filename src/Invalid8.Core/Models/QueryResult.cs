namespace Invalid8.Core;

/// <summary>
/// Result of a query operation with data and metadata
/// </summary>
/// <typeparam name="T">The type of the query result</typeparam>
public class QueryResult<T>
{
    /// <summary>
    /// The query result data
    /// </summary>
    public T? Data { get; set; }
    
    /// <summary>
    /// Whether the query executed successfully
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// Whether the data is stale and may be refreshed in the background
    /// </summary>
    public bool IsStale { get; set; }
    
    /// <summary>
    /// Whether the data was served from cache
    /// </summary>
    public bool IsFromCache { get; set; }
    
    /// <summary>
    /// When the data was cached (if from cache)
    /// </summary>
    public DateTime? CachedAt { get; set; }
    
    /// <summary>
    /// When the data becomes stale
    /// </summary>
    public DateTime? StaleAt { get; set; }
    
    /// <summary>
    /// When the data expires
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Error that occurred during query execution, if any
    /// </summary>
    public Exception? Error { get; set; }
    
    /// <summary>
    /// Duration of the query operation in milliseconds
    /// </summary>
    public long DurationMs { get; set; }
    
    /// <summary>
    /// ETag for concurrency control
    /// </summary>
    public string? ETag { get; set; }
    
    /// <summary>
    /// Creates a successful result from cached data
    /// </summary>
    public static QueryResult<T> FromCache(T data, DateTime cachedAt, DateTime staleAt, DateTime expiresAt) =>
        new() { Data = data, IsFromCache = true, IsSuccess = true, CachedAt = cachedAt, StaleAt = staleAt, ExpiresAt = expiresAt };
    
    /// <summary>
    /// Creates a successful result from fresh data
    /// </summary>
    public static QueryResult<T> FromFresh(T data) =>
        new() { Data = data, IsSuccess = true, IsFromCache = false };
    
    /// <summary>
    /// Creates a failed result with error
    /// </summary>
    public static QueryResult<T> FromError(Exception error) =>
        new() { Error = error, IsSuccess = false };
}