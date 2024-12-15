using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Utils
{
    public static class CollectionExtensions
    {
        public static void AddRangeIfNotNull<T>(this List<T> target, IEnumerable<T>? source)
        {
            if (source == null) 
                return;

            target.AddRange(source);
        }
    }
}
