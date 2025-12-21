using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace Hydra.Utils
{
    public static class DependencySorter
    {
        public static List<Type> SortByDependency(IEnumerable<Type> entityTypes)
        {
            var types = entityTypes.ToList();
            var adjacencyList = new Dictionary<Type, HashSet<Type>>();
            var inDegree = new Dictionary<Type, int>();

            // Initialize graph
            foreach (var type in types)
            {
                adjacencyList[type] = new HashSet<Type>();
                inDegree[type] = 0;
            }

            // Build graph based on Foreign Keys
            foreach (var type in types)
            {
                var properties = ReflectionHelper.GetCachedProperties(type);
                
                foreach (var prop in properties)
                {
                    // Check for ForeignKey attribute
                    var fkAttr = ReflectionHelper.GetAttribute<ForeignKeyAttribute>(prop);
                    if (fkAttr != null)
                    {
                        // The property related to this FK
                        var navigationPropertyName = fkAttr.Name;
                        var navigationProperty = properties.FirstOrDefault(p => p.Name == navigationPropertyName);

                        if (navigationProperty != null)
                        {
                             var relatedType = navigationProperty.PropertyType;
                             
                             // Handle collections (e.g. List<Order>) - usually not the FK side but good to check
                             if (relatedType.IsGenericType && typeof(System.Collections.IEnumerable).IsAssignableFrom(relatedType))
                             {
                                 relatedType = relatedType.GetGenericArguments()[0];
                             }

                             // If the related type is in our list of entities to sort...
                             if (types.Contains(relatedType) && relatedType != type)
                             {
                                 // "type" depends on "relatedType"
                                 // Edge: relatedType -> type
                                 if (!adjacencyList[relatedType].Contains(type))
                                 {
                                     adjacencyList[relatedType].Add(type);
                                     inDegree[type]++;
                                 }
                             }
                        }
                    }
                }
            }

            // Kahn's Algorithm for Topological Sort
            var queue = new Queue<Type>(inDegree.Where(kvp => kvp.Value == 0).Select(kvp => kvp.Key));
            var sortedList = new List<Type>();

            while (queue.Any())
            {
                var current = queue.Dequeue();
                sortedList.Add(current);

                if (adjacencyList.TryGetValue(current, out var neighbors))
                {
                    foreach (var neighbor in neighbors)
                    {
                        inDegree[neighbor]--;
                        if (inDegree[neighbor] == 0)
                        {
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

            // Cycle detection (optional but good)
            if (sortedList.Count != types.Count)
            {
                var remaining = types.Except(sortedList).Select(t => t.Name);
                throw new InvalidOperationException($"Circular dependency detected or items missing in sort. Unsorted types: {string.Join(", ", remaining)}");
            }

            return sortedList;
        }
    }
}
