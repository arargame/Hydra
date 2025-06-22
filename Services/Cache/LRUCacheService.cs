using Hydra.CacheManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Services.Cache
{
    public class LRUCacheService<TKey, TValue> : IQueryableCacheService<TKey, TValue>
            where TKey : notnull
            where TValue : class
    {
        private readonly LRUCache<TKey, TValue> _cache;

        public LRUCacheService(int capacity)
        {
            _cache = new LRUCache<TKey, TValue>(capacity);
        }

        public void Add(TKey key, TValue value, TimeSpan? ttl = null)
        {
            _cache.Add(key, value);
        }

        public bool TryGet(TKey key, out TValue value)
        {
            return _cache.TryGet(key, out value);
        }

        public void Remove(TKey key)
        {
            _cache.Remove(key);
        }

        public void Clear()
        {
            _cache.Clear();
        }

        public List<TValue> GetObjectList(Func<TValue, bool> predicate)
        {
            return _cache.GetAll().Where(predicate).ToList();
        }

        public bool TryGetAll(Func<TValue, bool> predicate, out List<TValue> values)
        {
            values = GetObjectList(predicate);
            return values.Count > 0;
        }
    }

}
