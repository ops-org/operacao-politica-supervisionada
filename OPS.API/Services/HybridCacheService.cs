using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using OPS.API.Configuration;
using System;
using System.Threading.Tasks;

namespace OPS.API.Services
{
    public interface IHybridCacheService
    {
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
        Task RemoveAsync(string key);
        Task RemoveByPrefixAsync(string prefix);
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

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            if (!_cacheSettings.Enabled)
            {
                return await factory();
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

            return await _hybridCache.GetOrCreateAsync<T>(key, async _ => await factory(), options: options);
        }

        public async Task RemoveAsync(string key)
        {
            if (_cacheSettings.Enabled)
            {
                await _hybridCache.RemoveAsync(key);
            }
        }

        public async Task RemoveByPrefixAsync(string prefix)
        {
            // Note: HybridCache doesn't have built-in prefix removal
            // This would need to be implemented with a custom solution
            // For now, we'll implement a simple approach using cache tags
            // This is a placeholder for a more sophisticated implementation
            await Task.CompletedTask;
        }
    }
}
