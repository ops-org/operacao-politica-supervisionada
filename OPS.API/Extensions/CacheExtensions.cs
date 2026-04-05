using System;
using System.Threading;
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
            Func<CancellationToken, Task<T>> factory,
            TimeSpan? expiration = null,
            CancellationToken ct = default)
        {
            if (!cacheService.IsEnabled)
            {
                return await factory(ct);
            }

            return await cacheService.GetOrCreateAsync(key, factory, expiration, ct);
        }

        public static async Task<T> GetOrCreateAsync<T>(
            this IHybridCacheService cacheService,
            string key,
            Func<CancellationToken, Task<T>> factory,
            TimeSpan? expiration = null,
            CancellationToken ct = default)
        {
            return await cacheService.GetOrCreateAsync(key, factory, expiration, ct);
        }
    }
}
