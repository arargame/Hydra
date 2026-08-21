using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Hydra.DTOs.ViewDTOs
{
    /// <summary>
    /// Resolves a ViewDTO type by name by scanning the loaded assemblies.
    /// Convention: "{Entity}DTO" first, then "{Entity}ViewDTO".
    /// Results (including misses) are cached.
    /// </summary>
    public static class ViewDTOTypeResolver
    {
        private static readonly ConcurrentDictionary<string, Type?> _cache = new();

        public static Type? Resolve(string? viewDTOTypeName, string? entityName = null)
        {
            var candidates = new List<string>();

            if (!string.IsNullOrWhiteSpace(viewDTOTypeName))
                candidates.Add(viewDTOTypeName!);

            if (!string.IsNullOrWhiteSpace(entityName))
            {
                candidates.Add($"{entityName}DTO");
                candidates.Add($"{entityName}ViewDTO");
            }

            foreach (var candidate in candidates.Distinct())
            {
                var type = _cache.GetOrAdd(candidate, FindType);

                if (type != null)
                    return type;
            }

            return null;
        }

        private static Type? FindType(string typeName)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Type.EmptyTypes; }
                })
                .FirstOrDefault(t => t.IsClass
                                  && !t.IsAbstract
                                  && typeof(ViewDTO).IsAssignableFrom(t)
                                  && t.Name == typeName);
        }
    }
}
