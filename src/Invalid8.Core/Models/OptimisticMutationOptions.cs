namespace Invalid8.Core;

/// <summary>
/// Options for optimistic mutations with UI update support
/// </summary>
/// <typeparam name="T">The type of the mutation result</typeparam>
public class OptimisticMutationOptions<T>
{
    /// <summary>
    /// Query keys to update optimistically before mutation completes
    /// </summary>
    public required string[][] QueryKeys { get; set; }
    
    /// <summary>
    /// Function to compute optimistic data for UI updates
    /// </summary>
    public required Func<T?> OptimisticData { get; set; }
    
    /// <summary>
    /// Function to update cache after successful mutation
    /// </summary>
    public Func<T, T>? OnSuccess { get; set; }
    
    /// <summary>
    /// Whether to automatically rollback optimistic update on error (default: true)
    /// </summary>
    public bool RollbackOnError { get; set; } = true;
    
    /// <summary>
    /// Custom error handler for mutation failures
    /// </summary>
    public Action<Exception>? OnError { get; set; }
    
    /// <summary>
    /// Timeout for the optimistic mutation operation
    /// </summary>
    public TimeSpan? Timeout { get; set; }
}