using Invalid8.Core;
using Invalid8.Core.Models;
using Invalid8.MediatR.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;


namespace Invalid8.MediatR;

public class MediatREventProvider(IMediator mediator, ILogger<MediatREventProvider> logger, IGenerateKey keyGenerator) : IEventProvider
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    private readonly IGenerateKey _keyGenerator = keyGenerator ?? throw new ArgumentNullException(nameof(keyGenerator));
    private readonly ILogger<MediatREventProvider> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ConcurrentBag<Func<CacheInvalidationEvent, Task>> _invalidationSubscribers = [];
    private readonly ConcurrentBag<Func<CacheUpdatedEvent, Task>> _updateSubscribers = [];

    public async Task PublishAsync(CacheInvalidationEvent @event, CancellationToken ct = default)
    {
        try
        {
            var tasks = _invalidationSubscribers.Select(subscriber => subscriber(@event)).ToArray();
            await Task.WhenAll(tasks).ConfigureAwait(false);
            await _mediator.Publish(new MediatRCacheInvalidationEvent(@event), ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish CacheInvalidationEvent for key: {CacheKey}", _keyGenerator.Generate(@event.Key));
            throw;
        }
    }

    public async Task PublishAsync(CacheUpdatedEvent @event, CancellationToken ct = default)
    {
        try
        {
            var tasks = _updateSubscribers.Select(subscriber => subscriber(@event)).ToArray();
            await Task.WhenAll(tasks).ConfigureAwait(false);
            await _mediator.Publish(new MediatRCacheUpdatedEvent(@event), ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish CacheUpdatedEvent for key: {CacheKey}", _keyGenerator.Generate(@event.Key));
            throw;
        }
    }


    public Task SubscribeAsync(Func<CacheInvalidationEvent, Task> handler, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(handler);

        _invalidationSubscribers.Add(handler);
        return Task.CompletedTask;
    }

    public Task SubscribeAsync(Func<CacheUpdatedEvent, Task> handler, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(handler);

        _updateSubscribers.Add(handler);
        return Task.CompletedTask;
    }
}
