using Invalid8.Core;
using Invalid8.MediatR.Events;
using MediatR;

namespace Invalid8.MediatR;

public class CacheInvalidationHandler(ICacheProvider cacheProvider) : INotificationHandler<MediatRCacheInvalidationEvent>
{
    private readonly ICacheProvider _cacheProvider = cacheProvider;

    public async Task Handle(MediatRCacheInvalidationEvent notification, CancellationToken cancellationToken)
    {
        await _cacheProvider.InvalidateAsync(notification.Event.Key, cancellationToken);
    }
}