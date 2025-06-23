using Invalid8.Core;
using Invalid8.Providers.Cache.Redis;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;

namespace Invalid8.Extensions;

public static class CacheServiceCollectionExtensions
{
    /// <summary>
    /// Adds Invalid8 with Redis cache provider and default key generator
    /// </summary>
    public static IServiceCollection AddInvalid8WithRedisCache(this IServiceCollection services, string redisConfigString,
        string instanceName = "invalid8", bool useDefaultKeyGenerator = true)
    {
        services.AddInvalid8WithRedisCache(options =>
        {
            options.Configuration = redisConfigString;
            options.InstanceName = instanceName;
        }, useDefaultKeyGenerator);

        return services;
    }

    /// <summary>
    /// Adds Invalid8 with Redis cache provider and default key generator
    /// </summary>
    public static IServiceCollection AddInvalid8WithRedisCache(this IServiceCollection services,
        Action<RedisCacheOptions> setupAction, bool useDefaultKeyGenerator = true)
    {
        if (useDefaultKeyGenerator) services.AddDefaultKeyGenerator();

        // Register IDistributedCache using StackExchange.Redis
        services.AddStackExchangeRedisCache(setupAction);

        // Register your custom provider
        services.AddSingleton<ICacheProvider, RedisCacheProvider>();

        return services;
    }
}