using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.CacheManagement
{
    public class TimedCache<TKey, TValue> where TValue : class
    {
        private readonly ConcurrentDictionary<TKey, CacheItem> cache;
        private readonly Timer cleanupTimer;
        private readonly object lockObject = new object();

        public TimedCache(TimeSpan cleanupInterval)
        {
            cache = new ConcurrentDictionary<TKey, CacheItem>();
            cleanupTimer = new Timer(CleanupExpiredItems, null, cleanupInterval, cleanupInterval);
        }

        public void Add(TKey key, TValue value, TimeSpan? timeToLive = null)
        {
            var expiration = timeToLive.HasValue ? DateTime.UtcNow.Add(timeToLive.Value) : DateTime.MaxValue;
            var cacheItem = new CacheItem(value, expiration);

            lock (lockObject)
            {
                cache[key] = cacheItem;
            }
        }

        public bool TryGet(TKey key, out TValue value)
        {
            lock (lockObject)
            {
                if (cache.TryGetValue(key, out var cacheItem) && cacheItem.Expiration > DateTime.UtcNow)
                {
                    value = cacheItem.Value;
                    return true;
                }

                value = null;
                return false;
            }
        }

        public void Remove(TKey key)
        {
            lock (lockObject)
            {
                cache.TryRemove(key, out _);
            }
        }

        public void Clear()
        {
            lock (lockObject)
            {
                cache.Clear();
            }
        }

        private void CleanupExpiredItems(object state)
        {
            lock (lockObject)
            {
                var expiredKeys = cache.Where(kvp => kvp.Value.Expiration <= DateTime.UtcNow).Select(kvp => kvp.Key).ToList();
                foreach (var key in expiredKeys)
                {
                    cache.TryRemove(key, out _);
                }
            }
        }

        private class CacheItem
        {
            public TValue Value { get; }
            public DateTime Expiration { get; }

            public CacheItem(TValue value, DateTime expiration)
            {
                Value = value;
                Expiration = expiration;
            }
        }
    }
}
