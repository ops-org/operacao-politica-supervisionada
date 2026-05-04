using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using OPS.API.Configuration;

namespace OPS.API.Services
{
    public interface IHybridCacheService
    {
        Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan? expiration = null, CancellationToken ct = default);
        Task RemoveAsync(string key, CancellationToken ct = default);
        Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default);
        bool IsEnabled { get; }
    }

    public class HybridCacheService : IHybridCacheService
    {
        private readonly HybridCache _hybridCache;
        private readonly CacheSettings _cacheSettings;

        public HybridCacheService(HybridCache hybridCache, IOptions<CacheSettings> cacheSettings)
        {
            _hybridCache = hybridCache;
            _cacheSettings = cacheSettings.Value;
        }

        public bool IsEnabled => _cacheSettings.Enabled;

        public async Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan? expiration = null, CancellationToken ct = default)
        {
            if (!_cacheSettings.Enabled)
            {
                return await factory(ct);
            }

            var options = expiration.HasValue
                ? new HybridCacheEntryOptions
                {
                    Expiration = expiration.Value,
                    LocalCacheExpiration = expiration.Value
                }
                : new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromHours(_cacheSettings.DefaultExpirationHours),
                    LocalCacheExpiration = TimeSpan.FromHours(_cacheSettings.LocalCacheExpirationHours)
                };

            return await _hybridCache.GetOrCreateAsync<T>(key, async (cancellationToken) => await factory(cancellationToken), options: options, cancellationToken: ct);
        }

        public async Task RemoveAsync(string key, CancellationToken ct = default)
        {
            if (_cacheSettings.Enabled)
            {
                await _hybridCache.RemoveAsync(key, ct);
            }
        }

        public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
        {
            // Note: HybridCache doesn't have built-in prefix removal
            // This would need to be implemented with a custom solution
            // For now, we'll implement a simple approach using cache tags
            // This is a placeholder for a more sophisticated implementation
            await Task.CompletedTask;
        }
    }
}
