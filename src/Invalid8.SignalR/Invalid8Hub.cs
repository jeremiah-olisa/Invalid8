using Invalid8.Core;
using Invalid8.SignalR.Constants;
using Invalid8.SignalR.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Invalid8.SignalR;

public class Invalid8Hub(IGenerateKey keyGenerator) : Hub, IInvalid8Hub
{
    public Task CacheInvalidated(string[] key, DateTime timestamp)
    {
        string cacheKey = keyGenerator.Generate(key);
        return Clients.All.SendAsync(HubEvent.CacheInvalidated.Name, cacheKey, timestamp);
    }
}
