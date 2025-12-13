using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.CacheManagement
{
    public class LRUCache<TKey, TValue> where TValue : class
    {
        private readonly int capacity;
        private readonly ConcurrentDictionary<TKey, LinkedListNode<CacheItem>> cache;
        private readonly LinkedList<CacheItem> lruList;
        private readonly object lockObject = new object();

        public LRUCache(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentException("Capacity must be greater than 0.", nameof(capacity));

            this.capacity = capacity;
            cache = new ConcurrentDictionary<TKey, LinkedListNode<CacheItem>>();
            lruList = new LinkedList<CacheItem>();
        }

        public void Add(TKey key, TValue value)
        {
            lock (lockObject)
            {
                if (cache.TryGetValue(key, out var existingNode))
                {
                    // Move the node to the front of the list
                    lruList.Remove(existingNode);
                    lruList.AddFirst(existingNode);
                    existingNode.Value.Value = value;
                }
                else
                {
                    if (cache.Count >= capacity)
                    {
                        // Remove the least recently used item
                        var lru = lruList.Last;
                        if (lru != null)
                        {
                            lruList.RemoveLast();
                            cache.TryRemove(lru.Value.Key, out _);
                        }
                    }

                    var cacheItem = new CacheItem(key, value);
                    var node = new LinkedListNode<CacheItem>(cacheItem);
                    lruList.AddFirst(node);
                    cache[key] = node;
                }
            }
        }

        public bool TryGet(TKey key, out TValue value)
        {
            lock (lockObject)
            {
                if (cache.TryGetValue(key, out var node))
                {
                    // Move the accessed node to the front of the list
                    lruList.Remove(node);
                    lruList.AddFirst(node);

                    value = node.Value.Value;
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
                if (cache.TryRemove(key, out var node))
                {
                    lruList.Remove(node);
                }
            }
        }

        public void Clear()
        {
            lock (lockObject)
            {
                cache.Clear();
                lruList.Clear();
            }
        }

        public IEnumerable<TValue> GetAll()
        {
            lock (lockObject)
            {
                return lruList.Select(cacheItem => cacheItem.Value).ToList();
            }
        }


        public IEnumerable<TValue> GetAll(Func<TValue, bool> predicate)
        {
            lock (lockObject)
            {
                return lruList
                    .Select(cacheItem => cacheItem.Value)
                    .Where(predicate)
                    .ToList();
            }
        }

        public bool TryGetAll(Func<TValue, bool> predicate, out List<TValue> values)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            lock (lockObject)
            {
                values = lruList
                    .Select(node => node.Value)
                    .Where(predicate)
                    .ToList();
            }

            return values.Any();
        }



        public int Count
        {
            get
            {
                lock (lockObject)
                {
                    return cache.Count;
                }
            }
        }

        private class CacheItem
        {
            public TKey Key { get; }
            public TValue Value { get; set; }

            public CacheItem(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            }
        }
    }
}
