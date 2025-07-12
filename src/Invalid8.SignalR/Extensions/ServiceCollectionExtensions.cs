using Invalid8.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace Invalid8.SignalR.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSignalREventProvider(this IServiceCollection services)
    {
        services.AddSignalRCore();
        services.AddSignalRQueryInvalidation();
        services.AddSingleton<IEventProvider, SignalREventProvider>();
        return services;
    }

    public static IServiceCollection AddSignalRQueryInvalidation(this IServiceCollection services)
    {
        services.AddHostedService<SignalRCacheInvalidationHandler>();
        return services;
    }   
}
