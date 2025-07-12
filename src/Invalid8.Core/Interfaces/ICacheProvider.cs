namespace Invalid8.Core;

public interface ICacheProvider
{
    /// <summary>
    /// Retrieves a value from the cache using the specified composite key.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">
    /// Composite key parts that will be joined with ':' delimiter.
    /// Example: ["users", "123"] becomes "users:123".
    /// </param>
    /// <param name="ct">
    /// Optional cancellation token to abort the operation.
    /// Throws <see cref="OperationCanceledException"/> if triggered.
    /// </param>
    /// <returns>
    /// A Task that resolves to:
    /// - The cached value if found and valid
    /// - null if the key doesn't exist or the entry is expired
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if key is null.</exception>
    /// <exception cref="ArgumentException">Thrown if key is empty.</exception>
    /// <example>
    /// <code>
    /// var user = await cache.GetAsync&lt;User&gt;(new[] {"users", userId});
    /// if (user == null) {
    ///     // Cache miss handling
    /// }
    /// </code>
    /// </example>
    Task<T?> GetAsync<T>(string[] key, CancellationToken ct = default);

    /// <summary>
    /// Stores a value in the cache with the specified composite key and options.
    /// </summary>
    /// <typeparam name="T">The type of the value to cache.</typeparam>
    /// <param name="key">
    /// Composite key parts that will be joined with ':' delimiter.
    /// Example: ["products", "456"] becomes "products:456".
    /// </param>
    /// <param name="value">The value to cache.</param>
    /// <param name="options">
    /// Cache configuration options including:
    /// - Absolute expiration
    /// - Sliding expiration
    /// - Priority
    /// - Dependency tracking
    /// </param>
    /// <param name="ct">
    /// Optional cancellation token to abort the operation.
    /// Throws <see cref="OperationCanceledException"/> if triggered.
    /// </param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if key or options is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if key is empty or contains invalid characters.
    /// </exception>
    /// <example>
    /// <code>
    /// await cache.SetAsync(
    ///     new[] {"products", product.Id},
    ///     product,
    ///     new CacheQueryOptions {
    ///         AbsoluteExpiration = DateTimeOffset.Now.AddHours(1)
    ///     });
    /// </code>
    /// </example>
    Task SetAsync<T>(string[] key, T value, CacheQueryOptions options, CancellationToken ct = default);

    /// <summary>
    /// Removes an item from the cache using the specified composite key.
    /// </summary>
    /// <param name="key">
    /// Composite key parts that will be joined with ':' delimiter.
    /// Example: ["sessions", "abc123"] removes "sessions:abc123".
    /// </param>
    /// <param name="ct">
    /// Optional cancellation token to abort the operation.
    /// Throws <see cref="OperationCanceledException"/> if triggered.
    /// </param>
    /// <returns>
    /// A Task representing the asynchronous operation.
    /// The operation completes successfully even if the key didn't exist.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if key is null.</exception>
    /// <exception cref="ArgumentException">Thrown if key is empty.</exception>
    /// <example>
    /// <code>
    /// // Basic invalidation
    /// await cache.InvalidateAsync(new[] {"users", "123"});
    ///
    /// // With cancellation
    /// var cts = new CancellationTokenSource();
    /// await cache.InvalidateAsync(new[] {"temp-data"}, cts.Token);
    /// </code>
    /// </example>
    Task InvalidateAsync(string[] key, CancellationToken ct = default);

    /// <summary>
    /// Invalidates all cache entries whose keys match the specified pattern using regular expression matching.
    /// This method provides flexible cache invalidation capabilities using regex patterns.
    /// </summary>
    /// <param name="pattern">
    /// The pattern to match against cache keys. 
    /// Can be either:
    /// - A simple wildcard pattern (will be converted to regex)
    ///   - * matches 0 or more characters
    ///   - ? matches exactly 1 character
    ///   - . matches literal dot (automatically escaped)
    /// - A full regular expression pattern
    /// 
    /// Examples:
    ///   "user:*"       - Matches all keys starting with "user:"
    ///   "*:profile"    - Matches all keys ending with ":profile"
    ///   "product.*.id" - Matches keys with literal dot wildcards
    ///   "^app\\..*"    - Raw regex matching keys starting with "app."
    /// </param>
    /// <param name="ct">
    /// The cancellation token that can be used to cancel the operation.
    /// The method will throw OperationCanceledException if cancellation is requested.
    /// </param>
    /// <returns>
    /// A Task that represents the asynchronous operation.
    /// The task always completes successfully (though some matching keys might not exist).
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the pattern parameter is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the pattern parameter is empty or whitespace.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown if the operation is cancelled via the cancellation token.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Performance Considerations:
    /// - This operation requires scanning all cache keys, which may impact performance in large caches.
    /// - For high-traffic scenarios, consider alternative invalidation strategies like:
    ///   - Tag-based invalidation
    ///   - Namespace/prefix invalidation
    ///   - Maintaining a secondary index of keys
    /// </para>
    /// <para>
    /// Thread Safety:
    /// - The operation is thread-safe if the underlying cache implementation is thread-safe.
    /// - Concurrent modifications to the cache during execution may cause unpredictable results.
    /// </para>
    /// <para>
    /// Pattern Matching Behavior:
    /// - By default, performs case-sensitive matching.
    /// - Simple wildcard patterns (*, ?) are converted to equivalent regex patterns.
    /// - For advanced regex features, provide a full regex pattern.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Simple wildcard pattern
    /// await cache.InvalidateByPatternAsync("session:*");
    /// 
    /// // Full regex pattern
    /// await cache.InvalidateByPatternAsync("^temp\\..*");
    /// 
    /// // With cancellation
    /// var cts = new CancellationTokenSource();
    /// await cache.InvalidateByPatternAsync("cache:*", cts.Token);
    /// </code>
    /// </example>
    Task InvalidateByPatternAsync(string pattern, CancellationToken ct = default);

    Task<CacheEntryMetadata?> GetEntryMetadataAsync(string[] key, CancellationToken ct = default);

}
