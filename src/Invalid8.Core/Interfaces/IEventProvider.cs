namespace Invalid8.Core;

/// <summary>
/// Event provider for distributed cache synchronization and notifications
/// </summary>
public interface IEventProvider : IAsyncDisposable
{
    /// <summary>
    /// Publishes a cache invalidation event
    /// </summary>
    Task PublishInvalidationAsync(CacheInvalidationEvent @event, CancellationToken ct = default);
    
    /// <summary>
    /// Publishes a cache update event
    /// </summary>
    Task PublishUpdateAsync(CacheUpdatedEvent @event, CancellationToken ct = default);
    
    /// <summary>
    /// Subscribes to cache invalidation events
    /// </summary>
    Task SubscribeToInvalidationsAsync(Func<CacheInvalidationEvent, Task> handler, CancellationToken ct = default);
    
    /// <summary>
    /// Subscribes to cache update events
    /// </summary>
    Task SubscribeToUpdatesAsync(Func<CacheUpdatedEvent, Task> handler, CancellationToken ct = default);
    
    /// <summary>
    /// Checks if the event provider is connected and healthy
    /// </summary>
    Task<bool> IsConnectedAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Ensures a connection to the event bus is established
    /// </summary>
    Task EnsureConnectedAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Event raised when the connection state changes
    /// </summary>
    event EventHandler<EventProviderConnectionEventArgs>? ConnectionStateChanged;
}