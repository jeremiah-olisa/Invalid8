using Invalid8.Core;

namespace Invalid8.Samples.WebApi;

public class CacheInvalidationSubscriber : IHostedService
{
    private readonly IEventProvider _eventProvider;
    private readonly ILogger<CacheInvalidationSubscriber> _logger;

    public CacheInvalidationSubscriber(IEventProvider eventProvider, ILogger<CacheInvalidationSubscriber> logger)
    {
        _eventProvider = eventProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _eventProvider.SubscribeAsync(@event =>
        {
            _logger.LogInformation("Cache invalidated for key: {CacheKey} at {Timestamp}",
                string.Join(":", @event.Key), @event.Timestamp);

            return Task.CompletedTask;
        }, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}