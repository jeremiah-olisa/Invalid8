using Microsoft.AspNetCore.SignalR;

namespace Invalid8.SignalR.Interfaces
{
    public interface IInvalid8Hub : Hub
    {
        Task CacheInvalidated(string[] key, DateTime timestamp);
    }
}