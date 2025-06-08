using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Services.Cache
{
    public interface ICacheService<TKey, TValue>
    where TKey : notnull
    where TValue : class
    {
        void Add(TKey key, TValue value, TimeSpan? ttl = null);
        bool TryGet(TKey key, out TValue value);
        void Remove(TKey key);
        void Clear();
    }
}
