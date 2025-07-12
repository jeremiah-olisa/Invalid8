using Microsoft.AspNetCore.SignalR;

namespace Invalid8.SignalR.Interfaces;

public class IInvalid8Hub : Hub
{
    Task CacheInvalidated(string[] key, DateTime timestamp)
    {
        return Clients.All.SendAsync("CacheInvalidated", key, timestamp);
    }
}