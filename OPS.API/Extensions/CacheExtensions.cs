using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Hybrid;
using OPS.API.Services;

namespace OPS.API.Extensions
{
    public static class CacheExtensions
    {
        public static async Task<T> GetOrCreateAsync<T>(
            this HybridCache hybridCache,
            IHybridCacheService cacheService,
            string key,
            Func<Task<T>> factory,
            TimeSpan? expiration = null)
        {
            if (!cacheService.IsEnabled)
            {
                return await factory();
            }

            return await cacheService.GetOrCreateAsync(key, factory, expiration);
        }

        public static async Task<T> GetOrCreateAsync<T>(
            this IHybridCacheService cacheService,
            string key,
            Func<Task<T>> factory,
            TimeSpan? expiration = null)
        {
            return await cacheService.GetOrCreateAsync(key, factory, expiration);
        }
    }
}
