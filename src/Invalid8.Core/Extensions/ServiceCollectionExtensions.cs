using Invalid8.Core.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Invalid8.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddQueryClient(this IServiceCollection services)
    {
        services.AddSingleton<IQueryClient, QueryClient>();
        return services;
    }

    public static IServiceCollection AddInvalid8(this IServiceCollection services)
    {
        AddQueryClient(services);
        services.AddSingleton<IGenerateKey, CacheKeyGenerator>();
        return services;
    }
}
