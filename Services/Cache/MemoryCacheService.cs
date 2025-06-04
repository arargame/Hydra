using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Services.Cache
{
    using Microsoft.Extensions.Caching.Memory;

    public class MemoryCacheService<TKey, TValue> : ICacheService<TKey, TValue>
        where TKey : notnull
        where TValue : class
    {
        private readonly IMemoryCache _memoryCache;
        private readonly TimeSpan _defaultTtl;

        public MemoryCacheService(IMemoryCache memoryCache, TimeSpan defaultTtl)
        {
            _memoryCache = memoryCache;
            _defaultTtl = defaultTtl;
        }

        public void Add(TKey key, TValue value, TimeSpan? ttl = null)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl ?? _defaultTtl
            };
            _memoryCache.Set(key, value, options);
        }

        public bool TryGet(TKey key, out TValue value)
        {
            if (_memoryCache.TryGetValue(key, out var cached) && cached is TValue typedValue)
            {
                value = typedValue;
                return true;
            }

            value = null!;
            return false;
        }

        public void Remove(TKey key)
        {
            _memoryCache.Remove(key);
        }

        public void Clear()
        {
            // IMemoryCache has no direct Clear method, workaround:
            // create your own instance if needed per scope and reset it
            throw new NotSupportedException("MemoryCache cannot be fully cleared unless scoped custom instance is used.");
        }
    }

}
