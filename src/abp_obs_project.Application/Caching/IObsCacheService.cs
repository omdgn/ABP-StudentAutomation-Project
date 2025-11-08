using System;
using System.Threading.Tasks;

namespace abp_obs_project.Caching;

/// <summary>
/// Generic cache service interface for OBS entities
/// </summary>
public interface IObsCacheService
{
    /// <summary>
    /// Get value from cache or set it using the factory function
    /// </summary>
    /// <typeparam name="T">Type of cached item</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="factory">Function to generate value if not in cache</param>
    /// <param name="expiration">Cache expiration time (default: 2 minutes)</param>
    /// <returns>Cached or newly generated value</returns>
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);

    /// <summary>
    /// Remove specific key from cache
    /// </summary>
    /// <param name="key">Cache key to remove</param>
    Task RemoveAsync(string key);

    /// <summary>
    /// Remove all keys matching a pattern (e.g., "obs:students:*")
    /// </summary>
    /// <param name="pattern">Key pattern to match</param>
    Task RemoveByPatternAsync(string pattern);
}
