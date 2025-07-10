using Invalid8.Core;
using MediatR;
using System.Collections.Concurrent;
namespace Invalid8.MediatR;


public class MediatREventProvider : IEventProvider
{
    private readonly IMediator _mediator;
    private readonly ConcurrentBag<Func<CacheInvalidationEvent, Task>> _subscribers;

    public MediatREventProvider(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _subscribers = [];
    }

    public async Task PublishAsync(CacheInvalidationEvent @event, CancellationToken ct = default)
    {
        // Publish to MediatR handlers
        await _mediator.Publish(new MediatRCacheInvalidationEvent(@event), ct);

        // Publish to manual subscribers
        foreach (var subscriber in _subscribers)
        {
            await subscriber(@event).ConfigureAwait(false);
        }
    }

    public Task SubscribeAsync(Func<CacheInvalidationEvent, Task> handler, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(handler);

        _subscribers.Add(handler);
        return Task.CompletedTask;
    }
}
