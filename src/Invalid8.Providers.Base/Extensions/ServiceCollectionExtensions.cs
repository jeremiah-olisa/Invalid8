using Invalid8.Core;
using Invalid8.Providers.Base;
using Microsoft.Extensions.DependencyInjection;

namespace Invalid8.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDefaultKeyGenerator(this IServiceCollection services)
    {
        services.AddSingleton<IKeyGenerator, CacheKeyGenerator>();

        return services;
    }
}