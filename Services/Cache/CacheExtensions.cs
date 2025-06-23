using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Services.Cache
{

    public static class CacheExtensions
    {
        /// <summary>
        /// Anahtar üzerinden mevcut veriyi kaldırır ve yenisini ekler.
        /// </summary>
        public static void Refresh<TKey, TValue>(
            this ICacheService<TKey, TValue> cache,
            TKey key,
            TValue value,
            TimeSpan? ttl = null)
            where TKey : notnull
            where TValue : class
        {
            cache.Remove(key);
            cache.Add(key, value, ttl);
        }

        /// <summary>
        /// Hata olması durumunda Refresh işlemini sessizce geçer.
        /// </summary>
        public static bool TryRefresh<TKey, TValue>(
            this ICacheService<TKey, TValue> cache,
            TKey key,
            TValue value,
            TimeSpan? ttl = null)
            where TKey : notnull
            where TValue : class
        {
            try
            {
                cache.Remove(key);
                cache.Add(key, value, ttl);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }


}
