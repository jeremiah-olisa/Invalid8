using Invalid8.Core;
using Invalid8.Providers.Base;

namespace Invalid8.Providers.Cache.Redis;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

/// <summary>
/// Redis implementation of ICacheProvider using IBaseDistributedCache
/// </summary>
public class RedisCacheProvider(
    IDistributedCache distributedCache,
    IKeyGenerator keyGenerator,
    ILogger<RedisCacheProvider> logger)
    : BaseCacheProvider(keyGenerator, logger)
{
    public override byte[]? Get(string key)
    {
        try
        {
            return distributedCache.Get(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get value from Redis for key: {Key}", key);
            throw;
        }
    }

    public override async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
    {
        try
        {
            return await distributedCache.GetAsync(key, token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get value from Redis for key: {Key}", key);
            throw;
        }
    }

    public override void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        try
        {
            distributedCache.Set(key, value, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set value in Redis for key: {Key}", key);
            throw;
        }
    }

    public override async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options,
        CancellationToken token = default)
    {
        try
        {
            await distributedCache.SetAsync(key, value, options, token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set value in Redis for key: {Key}", key);
            throw;
        }
    }

    public override void Refresh(string key)
    {
        try
        {
            distributedCache.Refresh(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh value in Redis for key: {Key}", key);
            throw;
        }
    }

    public override async Task RefreshAsync(string key, CancellationToken token = default)
    {
        try
        {
            await distributedCache.RefreshAsync(key, token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh value in Redis for key: {Key}", key);
            throw;
        }
    }

    public override void Remove(string key)
    {
        try
        {
            distributedCache.Remove(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove value from Redis for key: {Key}", key);
            throw;
        }
    }

    public override async Task RemoveAsync(string key, CancellationToken token = default)
    {
        try
        {
            await distributedCache.RemoveAsync(key, token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove value from Redis for key: {Key}", key);
            throw;
        }
    }
}