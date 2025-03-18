using Microsoft.Extensions.Caching.Memory;

namespace Si.CoreHub.MemoryCache
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public Task<bool> Exists<TKey>(TKey key)
        {
            return Task.FromResult(_memoryCache.TryGetValue(key, out _));
        }

        public Task<TValue> Get<TKey, TValue>(TKey key)
        {
            if (_memoryCache.TryGetValue(key, out TValue value))
            {
                Task.FromResult(value);
            }
            return Task.FromResult(default(TValue));
        }

        public Task<TValue> GetAndRemove<TKey, TValue>(TKey key)
        {
            if (_memoryCache.TryGetValue(key, out TValue value))
            {
                _memoryCache.Remove(key);
                return Task.FromResult(value);
            }
            return Task.FromResult(default(TValue)); ;
        }

        public Task Remove<TKey>(TKey key)
        {
            _memoryCache.Remove(key);
            return Task.CompletedTask;
        }

        public Task<TValue> SetAbsolute<TKey, TValue>(TKey key, TValue value, TimeSpan? AbsoluteExpiration = null)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = AbsoluteExpiration
            };
            _memoryCache.Set(key, value, cacheEntryOptions);
            return Task.FromResult(value);
        }

        public Task<TValue> SetSliding<TKey, TValue>(TKey key, TValue value, TimeSpan? SlidingExpiration = null)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                SlidingExpiration = SlidingExpiration
            };
            _memoryCache.Set(key, value, cacheEntryOptions);
            return Task.FromResult(value);
        }
    }
}
