using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace LeanCode.Cache.AspNet
{
    internal class InMemoryCacheAdapter : ICacher
    {
        private readonly IMemoryCache cache;

        public InMemoryCacheAdapter(IMemoryCache cache)
        {
            this.cache = cache;
        }

        public void AddOrUpdate<T>(string key, T value)
            where T : class
        {
            cache.Set(key, value);
        }

        public T Get<T>(string key)
            where T : class
        {
            return cache.Get<T>(key);
        }

        public T GetOrCreate<T>(string key, Func<T> getValueToCache)
            where T : class
        {
            return cache.GetOrCreate(key, _ => getValueToCache());
        }

        public T GetOrCreate<T>(string key, TimeSpan duration, Func<T> getValueToCache)
            where T : class
        {
            return cache.GetOrCreate(key, e =>
            {
                e.AbsoluteExpirationRelativeToNow = duration;
                return getValueToCache();
            });
        }

        public Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> getValueToCache)
            where T : class
        {
            return cache.GetOrCreateAsync(key, _ => getValueToCache());
        }

        public Task<T> GetOrCreateAsync<T>(string key, TimeSpan duration, Func<Task<T>> getValueToCache)
            where T : class
        {
            return cache.GetOrCreateAsync(key, e =>
            {
                e.AbsoluteExpirationRelativeToNow = duration;
                return getValueToCache();
            });
        }
    }
}
