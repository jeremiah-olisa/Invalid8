using Invalid8.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Invalid8.MediatR.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediatREventProvider(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));
        services.AddSingleton<IEventProvider, MediatREventProvider>();
        return services;
    }
}
