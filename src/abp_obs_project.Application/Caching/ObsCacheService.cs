using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace abp_obs_project.Caching;

/// <summary>
/// Redis-backed cache service implementation
/// </summary>
public class ObsCacheService : IObsCacheService, ITransientDependency
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<ObsCacheService> _logger;
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(2);

    public ObsCacheService(IDistributedCache cache, ILogger<ObsCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        var stopwatch = Stopwatch.StartNew();

        // Try to get from cache
        var cachedBytes = await _cache.GetAsync(key);

        if (cachedBytes != null)
        {
            var cachedJson = System.Text.Encoding.UTF8.GetString(cachedBytes);
            var cachedValue = JsonSerializer.Deserialize<T>(cachedJson);
            if (cachedValue != null)
            {
                stopwatch.Stop();
                _logger.LogInformation(
                    "✅ CACHE HIT: {Key} | Redis Read: {ElapsedMs}ms ⚡",
                    key,
                    stopwatch.ElapsedMilliseconds
                );
                return cachedValue;
            }
        }

        // Not in cache, generate value
        stopwatch.Restart();
        var value = await factory();
        stopwatch.Stop();

        _logger.LogInformation(
            "❌ CACHE MISS: {Key} | DB Query: {ElapsedMs}ms | Caching result...",
            key,
            stopwatch.ElapsedMilliseconds
        );

        if (value == null)
        {
            return default;
        }

        // Store in cache with expiration
        var json = JsonSerializer.Serialize(value);
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);

        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration
        };

        await _cache.SetAsync(key, bytes, cacheOptions);

        return value;
    }

    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync(key);
        _logger.LogInformation("🗑️ CACHE INVALIDATED: {Key}", key);
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        // Note: Pattern matching requires Redis-specific implementation
        // For now, we'll remove specific known keys
        // This is a simplified version; full pattern matching would need IConnectionMultiplexer
        await RemoveAsync(pattern);
    }
}
