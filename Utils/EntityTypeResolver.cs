using Hydra.Core;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Hydra.Utils
{
    /// <summary>
    /// Bir entity tipini ADINDAN bulur ("RolePermission" → typeof(RolePermission)).
    ///
    /// ViewDTOTypeResolver ile aynı fikir, farklı hedef: orası ekran konfigürasyon sınıflarını
    /// bulur, burası BaseObject'ten türeyen gerçek entity'leri. Generic köprü uçlarının
    /// (CollectionLinkService) her yeni ilişki için kod yazmadan çalışabilmesi buna dayanıyor.
    ///
    /// Yüklü assembly'ler taranır ve sonuç — bulunamayanlar dahil — cache'lenir; aksi hâlde her
    /// istek bütün assembly'leri yeniden gezerdi.
    /// </summary>
    public static class EntityTypeResolver
    {
        private static readonly ConcurrentDictionary<string, Type?> _cache = new();

        public static Type? Resolve(string? entityName)
        {
            if (string.IsNullOrWhiteSpace(entityName))
                return null;

            return _cache.GetOrAdd(entityName!, FindType);
        }

        private static Type? FindType(string entityName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;

                try
                {
                    types = assembly.GetTypes();
                }
                catch (System.Reflection.ReflectionTypeLoadException ex)
                {
                    // Kısmen yüklenebilen assembly'lerde elde olan tiplerle devam et.
                    types = ex.Types.Where(t => t is not null).ToArray()!;
                }
                catch
                {
                    continue;
                }

                var match = types.FirstOrDefault(t => t.IsClass
                                                      && !t.IsAbstract
                                                      && string.Equals(t.Name, entityName, StringComparison.Ordinal)
                                                      && InheritsFromBaseObject(t));

                if (match != null)
                    return match;
            }

            return null;
        }

        /// <summary>BaseObject&lt;T&gt; generic olduğu için miras zinciri elle yürünüyor.</summary>
        private static bool InheritsFromBaseObject(Type type)
        {
            var current = type.BaseType;

            while (current != null)
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(BaseObject<>))
                    return true;

                current = current.BaseType;
            }

            return false;
        }
    }
}
