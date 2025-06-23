namespace Invalid8.Core;

/// <summary>
/// Options for configuring mutation behavior and cache invalidation
/// </summary>
public class MutationOptions
{
    /// <summary>
    /// Query keys to invalidate after successful mutation
    /// </summary>
    public string[][]? InvalidateQueries { get; set; }
    
    /// <summary>
    /// Query keys to refetch after successful mutation
    /// </summary>
    public string[][]? RefetchQueries { get; set; }
    
    /// <summary>
    /// Whether to publish invalidation events to other instances (default: true)
    /// </summary>
    public bool PublishEvent { get; set; } = true;
    
    /// <summary>
    /// Timeout for the mutation operation
    /// </summary>
    public TimeSpan? Timeout { get; set; }
    
    /// <summary>
    /// Number of retry attempts for the mutation
    /// </summary>
    public int RetryCount { get; set; } = 3;
    
    /// <summary>
    /// Delay between retry attempts
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
    
    /// <summary>
    /// Whether to throw exceptions on mutation failure (default: true)
    /// </summary>
    public bool ThrowOnError { get; set; } = true;
}