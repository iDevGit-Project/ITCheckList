using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace ITCheckList.Services
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private static readonly ConcurrentDictionary<string, CacheMetadata> _trackingInfo = new();

        public CacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void SetWithTracking<T>(string key, T value, TimeSpan duration)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = duration
            };

            _cache.Set(key, value, options);
            _trackingInfo[key] = new CacheMetadata
            {
                Key = key,
                CreatedAt = DateTime.Now,
                Expiration = DateTime.Now.Add(duration),
                DataType = typeof(T).Name
            };
        }

        public T Get<T>(string key)
        {
            return _cache.TryGetValue(key, out T value) ? value : default;
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
            _trackingInfo.TryRemove(key, out _);
        }

        public bool Exists(string key)
        {
            return _cache.TryGetValue(key, out _);
        }

        public static IEnumerable<CacheMetadata> GetTrackedItems()
        {
            return _trackingInfo.Values;
        }

        public class CacheMetadata
        {
            public string Key { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime Expiration { get; set; }
            public string DataType { get; set; }
        }
    }
}
