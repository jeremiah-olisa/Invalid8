using Invalid8.Core;
using MediatR;

namespace Invalid8.MediatR.Events;

public class MediatRCacheInvalidationEvent(CacheInvalidationEvent @event) : INotification
{
    public CacheInvalidationEvent Event { get; } = @event;
}
