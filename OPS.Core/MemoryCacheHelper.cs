using System;
using System.Runtime.Caching;

namespace OPS.Core
{
	public static class MemoryCacheHelper
	{
		public static T GetCachedData<T>(string cacheKey) where T : class
		{
			//Returns null if the string does not exist, prevents a race condition where the cache invalidates between the contains check and the retreival.
			return MemoryCache.Default.Get(cacheKey, null) as T;
		}

		public static void AddCacheData<T>(string cacheKey, T cachedData, object cacheLock, int cacheTimePolicyMinutes = 240) where T : class
		{
			try
			{
				lock (cacheLock)
				{
					if (MemoryCache.Default.Get(cacheKey, null) != null)
					{
						MemoryCache.Default.Remove(cacheKey);
					}

					//The value still did not exist so we now write it in to the cache.
					CacheItemPolicy cip = new CacheItemPolicy()
					{
						AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddMinutes(cacheTimePolicyMinutes))
					};

					MemoryCache.Default.Set(cacheKey, cachedData, cip);
				}
			}
			catch (Exception)
			{
				// TODO: Logar Erro
			}
		}
	}
}