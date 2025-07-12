using Invalid8.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Invalid8.SignalR.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediatREventProvider(this IServiceCollection services)
    {
        services.AddSignalRCore();
        services.AddSingleton<IEventProvider, SignalREventProvider>();
        return services;
    }
}
