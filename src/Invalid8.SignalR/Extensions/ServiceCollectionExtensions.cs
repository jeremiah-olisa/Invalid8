using Invalid8.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
namespace Invalid8.SignalR.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSignalREventProvider(this IServiceCollection services)
    {
        services.AddSignalR();
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
