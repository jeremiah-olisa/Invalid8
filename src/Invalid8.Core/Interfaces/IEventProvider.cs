using Invalid8.Core.Models;

namespace Invalid8.Core;


public interface IEventProvider
{
    Task PublishAsync(CacheInvalidationEvent @event, CancellationToken ct = default);
    Task SubscribeAsync(Func<CacheInvalidationEvent, Task> handler, CancellationToken ct = default);

    Task PublishAsync(CacheUpdatedEvent @event, CancellationToken ct = default);
    Task SubscribeAsync(Func<CacheUpdatedEvent, Task> handler, CancellationToken ct = default);
}