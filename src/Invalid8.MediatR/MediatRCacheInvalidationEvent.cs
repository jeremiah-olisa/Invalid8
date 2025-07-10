using Invalid8.Core;
using MediatR;

namespace Invalid8.MediatR;

public class MediatRCacheInvalidationEvent : INotification
{
    public CacheInvalidationEvent Event { get; }

    public MediatRCacheInvalidationEvent(CacheInvalidationEvent @event)
    {
        Event = @event;
    }
}
