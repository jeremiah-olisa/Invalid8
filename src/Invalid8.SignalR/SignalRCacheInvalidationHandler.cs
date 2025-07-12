using Invalid8.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Invalid8.SignalR;

public class SignalRCacheInvalidationHandler(
    IEventProvider eventProvider,
    ICacheProvider cacheProvider,
    IGenerateKey keyGenerator,
    ILogger<SignalRCacheInvalidationHandler> logger) : IHostedService
{
    private readonly IEventProvider _eventProvider = eventProvider ?? throw new ArgumentNullException(nameof(eventProvider));
    private readonly ICacheProvider _cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));
    private readonly IGenerateKey _keyGenerator = keyGenerator ?? throw new ArgumentNullException(nameof(keyGenerator));
    private readonly ILogger<SignalRCacheInvalidationHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _eventProvider.SubscribeAsync(async (CacheInvalidationEvent @event) =>
        {
            await _cacheProvider.InvalidateAsync(@event.Key, cancellationToken);
            _logger.LogInformation("Invalidated cache for key: {CacheKey} at {Timestamp}", @event.Key, @event.Timestamp);
        }, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}