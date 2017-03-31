using System;
using System.Threading.Tasks;

namespace LeanCode.Cache
{
    public interface ICacher
    {
        void AddOrUpdate<T>(string key, T value) where T : class;
        T Get<T>(string key) where T : class;
        T GetOrCreate<T>(string key, Func<T> getValueToCache) where T : class;
        T GetOrCreate<T>(string key, TimeSpan duration, Func<T> getValueToCache) where T : class;
        Task<T> GetOrCreate<T>(string key, Func<Task<T>> getValueToCache) where T : class;
        Task<T> GetOrCreate<T>(string key, TimeSpan duration, Func<Task<T>> getValueToCache) where T : class;
    }
}
