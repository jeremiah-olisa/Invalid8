using Invalid8.Core;
using MediatR;

namespace Invalid8.MediatR;

public class CacheInvalidationHandler : INotificationHandler<MediatRCacheInvalidationEvent>
{
    private readonly ICacheProvider _cacheProvider;

    public CacheInvalidationHandler(ICacheProvider cacheProvider)
    {
        _cacheProvider = cacheProvider;
    }

    public async Task Handle(MediatRCacheInvalidationEvent notification, CancellationToken cancellationToken)
    {
        await _cacheProvider.InvalidateAsync(notification.Event.Key, cancellationToken);
    }
}