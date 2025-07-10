using Invalid8.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Invalid8.InMemory.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInMemoryCacheProvider(this IServiceCollection services)
    {
        services.AddSingleton<ICacheProvider, InMemoryCacheProvider>();
        return services;
    }
}
