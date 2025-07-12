using Invalid8.Core;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Invalid8.Samples.WebApi;

public class CacheInvalidationSubscriber(IEventProvider eventProvider, IGenerateKey keyGenerator, ILogger<CacheInvalidationSubscriber> logger) : IHostedService
{
    private readonly IEventProvider _eventProvider = eventProvider ?? throw new ArgumentNullException(nameof(eventProvider));
    private readonly IGenerateKey _keyGenerator = keyGenerator ?? throw new ArgumentNullException(nameof(keyGenerator));
    private readonly ILogger<CacheInvalidationSubscriber> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task StartAsync(CancellationToken ct = default)
    {
        await _eventProvider.SubscribeAsync((CacheInvalidationEvent @event) =>
        {
            _logger.LogInformation("Cache invalidated for key: {CacheKey} at {Timestamp}", @event.Key, @event.Timestamp);

            return Task.CompletedTask;
        }, ct);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}