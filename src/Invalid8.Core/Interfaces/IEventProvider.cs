namespace Invalid8.Core;


public interface IEventProvider
{
    Task PublishAsync(CacheInvalidationEvent @event, CancellationToken ct = default);
    Task SubscribeAsync(Func<CacheInvalidationEvent, Task> handler, CancellationToken ct = default);
}