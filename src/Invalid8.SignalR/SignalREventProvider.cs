using Invalid8.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Invalid8.Core.Models;

namespace Invalid8.SignalR;

public class SignalREventProvider<T>(IHubContext<T> hubContext, ILogger<SignalREventProvider<T>> logger, IGenerateKey keyGenerator) : IEventProvider where T : Hub
{
    private readonly IHubContext<T> _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
    private readonly IGenerateKey _keyGenerator = keyGenerator ?? throw new ArgumentNullException(nameof(hubContext));
    private readonly ILogger<SignalREventProvider<T>> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ConcurrentBag<Func<CacheInvalidationEvent, Task>> _invalidationSubscribers = [];
    private readonly ConcurrentBag<Func<CacheUpdatedEvent, Task>> _updateSubscribers = [];

    public async Task PublishAsync(CacheInvalidationEvent @event, CancellationToken ct = default)
    {
        var cacheKey = _keyGenerator.Generate(@event.Key, ct);
        try
        {
            await _hubContext.Clients.All.SendAsync("CacheInvalidated", cacheKey, @event.Timestamp, ct);
            var tasks = _invalidationSubscribers.Select(subscriber => subscriber(@event)).ToArray();

            await Task.WhenAll(tasks).ConfigureAwait(false);

            _logger.LogInformation("Published CacheInvalidationEvent for key: {CacheKey} at {Timestamp}", cacheKey, @event.Timestamp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish CacheInvalidationEvent for key: {CacheKey}", cacheKey);
            throw;
        }
    }

    public async Task PublishAsync(CacheUpdatedEvent @event, CancellationToken ct = default)
    {
        var cacheKey = _keyGenerator.Generate(@event.Key, ct);
        try
        {
            await _hubContext.Clients.All.SendAsync("CacheUpdated", cacheKey, @event.Data, @event.Timestamp, ct);

            var tasks = _updateSubscribers.Select(subscriber => subscriber(@event)).ToArray();
            await Task.WhenAll(tasks).ConfigureAwait(false);

            _logger.LogInformation("Published CacheUpdatedEvent for key: {CacheKey} at {Timestamp}",
                cacheKey, @event.Timestamp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish CacheUpdatedEvent for key: {CacheKey}", cacheKey);
            throw;
        }
    }

    public Task SubscribeAsync(Func<CacheInvalidationEvent, Task> handler, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(handler);

        _invalidationSubscribers.Add(handler);
        _logger.LogInformation("Subscribed to CacheInvalidationEvent");

        return Task.CompletedTask;
    }

    public Task SubscribeAsync(Func<CacheUpdatedEvent, Task> handler, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(handler);

        _updateSubscribers.Add(handler);
        _logger.LogInformation("Subscribed to CacheUpdatedEvent");

        return Task.CompletedTask;
    }
}