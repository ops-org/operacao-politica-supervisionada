using System;
using System.Runtime.Caching;

namespace OPS.Core
{
    public static class CacheHelper
    {
        private static MemoryCache cache = new MemoryCache("CachingProvider");

        static readonly object padlock = new object();

        public static void Add<T>(string key, T value)
        {
            lock (padlock)
            {
                if (cache[key] != null)
                {
                    cache.Remove(key);
                }

                cache.Add(key, value, DateTimeOffset.MaxValue);
            }
        }

        public static void Remove(string key)
        {
            lock (padlock)
            {
                cache.Remove(key);
            }
        }

        public static T Get<T>(string key, bool remove = false)
        {
            lock (padlock)
            {
                var res = cache[key];

                if (res != null)
                {
                    if (remove == true)
                        cache.Remove(key);
                }

                return (T)res;
            }
        }
    }
}