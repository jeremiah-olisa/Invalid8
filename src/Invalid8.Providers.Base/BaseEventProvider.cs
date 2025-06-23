using Invalid8.Core;
using Invalid8.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace Invalid8.Providers.Base;

/// <summary>
/// Abstract base class for event providers with robust connection management,
/// error handling, and event processing infrastructure
/// </summary>
public abstract class BaseEventProvider(ILogger<BaseEventProvider> logger) : IEventProvider
{
    ~BaseEventProvider()
    {
        Dispose(disposing: false);
    }

    protected readonly ILogger<BaseEventProvider> _logger = logger;
    private readonly List<Func<CacheInvalidationEvent, Task>> _invalidationHandlers = new();
    private readonly List<Func<CacheUpdatedEvent, Task>> _updateHandlers = new();
    private bool _isDisposed;

    public event EventHandler<EventProviderConnectionEventArgs>? ConnectionStateChanged;

    // ----------------------------------------------------------------------------
    // IEventProvider Implementation (VIRTUAL - Common logic with override capability)
    // ----------------------------------------------------------------------------

    public virtual async Task PublishInvalidationAsync(CacheInvalidationEvent @event, CancellationToken ct = default)
    {
        EnsureNotDisposed();
        
        try
        {
            await EnsureConnectedAsync(ct);
            await PublishInvalidationCoreAsync(@event, ct);
            _logger.LogDebug("Published invalidation event for {KeyCount} keys", @event.Keys?.Length ?? 0);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to publish invalidation event");
            throw new EventPublishException("Failed to publish invalidation event", ex);
        }
    }

    public virtual async Task PublishUpdateAsync(CacheUpdatedEvent @event, CancellationToken ct = default)
    {
        EnsureNotDisposed();
        
        try
        {
            await EnsureConnectedAsync(ct);
            await PublishUpdateCoreAsync(@event, ct);
            _logger.LogDebug("Published update event for {KeyCount} keys", @event.Keys?.Length ?? 0);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to publish update event");
            throw new EventPublishException("Failed to publish update event", ex);
        }
    }

    public virtual async Task SubscribeToInvalidationsAsync(Func<CacheInvalidationEvent, Task> handler, CancellationToken ct = default)
    {
        EnsureNotDisposed();
        
        _invalidationHandlers.Add(handler);
        
        try
        {
            await EnsureConnectedAsync(ct);
            if (_invalidationHandlers.Count == 1) // First subscriber
            {
                await SubscribeToInvalidationsCoreAsync(HandleInvalidationEvent, ct);
            }
            _logger.LogDebug("Added invalidation event handler. Total handlers: {HandlerCount}", _invalidationHandlers.Count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _invalidationHandlers.Remove(handler);
            _logger.LogError(ex, "Failed to subscribe to invalidation events");
            throw new EventSubscriptionException("Failed to subscribe to invalidation events", ex);
        }
    }

    public virtual async Task SubscribeToUpdatesAsync(Func<CacheUpdatedEvent, Task> handler, CancellationToken ct = default)
    {
        EnsureNotDisposed();
        
        _updateHandlers.Add(handler);
        
        try
        {
            await EnsureConnectedAsync(ct);
            if (_updateHandlers.Count == 1) // First subscriber
            {
                await SubscribeToUpdatesCoreAsync(HandleUpdateEvent, ct);
            }
            _logger.LogDebug("Added update event handler. Total handlers: {HandlerCount}", _updateHandlers.Count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _updateHandlers.Remove(handler);
            _logger.LogError(ex, "Failed to subscribe to update events");
            throw new EventSubscriptionException("Failed to subscribe to update events", ex);
        }
    }

    public virtual async Task<bool> IsConnectedAsync(CancellationToken ct = default)
    {
        EnsureNotDisposed();
        
        try
        {
            return await CheckConnectionAsync(ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Failed to check connection status");
            return false;
        }
    }

    public virtual async Task EnsureConnectedAsync(CancellationToken ct = default)
    {
        EnsureNotDisposed();
        
        var wasConnected = await IsConnectedAsync(ct);
        if (wasConnected) return;

        try
        {
            await ConnectCoreAsync(ct);
            var isNowConnected = await IsConnectedAsync(ct);
            
            if (isNowConnected && !wasConnected)
            {
                OnConnectionStateChanged(true, wasConnected, null);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to establish connection");
            OnConnectionStateChanged(false, wasConnected, ex);
            throw new EventConnectionException("Failed to establish connection", ex);
        }
    }

    // ----------------------------------------------------------------------------
    // Derived classes (MUST implement abstract Methods)
    // ----------------------------------------------------------------------------

    protected abstract Task PublishInvalidationCoreAsync(CacheInvalidationEvent @event, CancellationToken ct);
    protected abstract Task PublishUpdateCoreAsync(CacheUpdatedEvent @event, CancellationToken ct);
    protected abstract Task SubscribeToInvalidationsCoreAsync(Func<CacheInvalidationEvent, Task> handler, CancellationToken ct);
    protected abstract Task SubscribeToUpdatesCoreAsync(Func<CacheUpdatedEvent, Task> handler, CancellationToken ct);
    protected abstract Task<bool> CheckConnectionAsync(CancellationToken ct);
    protected abstract Task ConnectCoreAsync(CancellationToken ct);
    protected abstract Task DisconnectCoreAsync(CancellationToken ct);

    // ----------------------------------------------------------------------------
    // Event Handling (PROTECTED - Available to derived classes)
    // ----------------------------------------------------------------------------

    protected virtual async Task HandleInvalidationEvent(CacheInvalidationEvent @event)
    {
        if (_invalidationHandlers.Count == 0) return;

        var tasks = _invalidationHandlers.Select(handler => 
            SafeInvokeHandlerAsync(handler, @event, "invalidation")).ToArray();

        await Task.WhenAll(tasks);
    }

    protected virtual async Task HandleUpdateEvent(CacheUpdatedEvent @event)
    {
        if (_updateHandlers.Count == 0) return;

        var tasks = _updateHandlers.Select(handler => 
            SafeInvokeHandlerAsync(handler, @event, "update")).ToArray();

        await Task.WhenAll(tasks);
    }

    protected virtual async Task SafeInvokeHandlerAsync<TEvent>(Func<TEvent, Task> handler, TEvent @event, string eventType)
    {
        try
        {
            await handler(@event);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Event handler failed for {EventType} event", eventType);
            // Don't throw - continue processing other handlers
        }
    }

    // ----------------------------------------------------------------------------
    // Connection State Management (PROTECTED)
    // ----------------------------------------------------------------------------

    protected virtual void OnConnectionStateChanged(bool isConnected, bool wasConnected, Exception? error)
    {
        ConnectionStateChanged?.Invoke(this, new EventProviderConnectionEventArgs
        {
            IsConnected = isConnected,
            WasConnected = wasConnected,
            Error = error,
            Timestamp = DateTime.UtcNow
        });

        var message = isConnected ? "Event provider connected" : "Event provider disconnected";
        var level = isConnected ? LogLevel.Information : LogLevel.Warning;
        
        _logger.Log(level, error, message);
    }

    // ----------------------------------------------------------------------------
    // Dispose Pattern
    // ----------------------------------------------------------------------------

    protected virtual void EnsureNotDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        try
        {
            await DisconnectCoreAsync(CancellationToken.None);
            _invalidationHandlers.Clear();
            _updateHandlers.Clear();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during event provider disposal");
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                // Sync dispose logic if needed
                _invalidationHandlers.Clear();
                _updateHandlers.Clear();
            }
            _isDisposed = true;
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
}
