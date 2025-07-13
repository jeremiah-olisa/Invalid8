namespace Invalid8.SignalR.Interfaces;

public interface IInvalid8Hub
{
    Task CacheInvalidated(string[] key, DateTime timestamp);
}