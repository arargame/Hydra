using Hydra.Core;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Utils
{
    public static class ReflectionHelper
    {
        /// <summary>
        /// Belirtilen tipteki bir nesnenin özelliğine değer atar.
        /// </summary>
        public static void SetPropertyValue<T>(T instance, string propertyName, object value)
        {
            var property = instance?.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property == null)
                throw new ArgumentException($"Property '{propertyName}' not found on type '{typeof(T)}'.");

            property.SetValue(instance, value);
        }

        /// <summary>
        /// Belirtilen tipteki bir nesnenin bir özelliğinin değerini döner.
        /// </summary>
        public static object? GetPropertyValue<T>(T instance, string propertyName)
        {
            var property = instance?.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property == null)
                throw new ArgumentException($"Property '{propertyName}' not found on type '{typeof(T)}'.");

            return property.GetValue(instance);
        }

        public static object? InvokeMethod(Type invokerType,
                                            object? invokerObject,
                                            string methodName,
                                            BindingFlags? bindingFlags = null,
                                            Type[]? argumentTypesOfMethod = null,
                                            Type[]? genericTypes = null,
                                            object?[]? parameters = null,
                                            Action<Exception>? logAction = null)
        {
            try
            {
                var methodInfo = GetMethodInfo(invokerType, methodName, bindingFlags, argumentTypesOfMethod, genericTypes);

                object? result;

                result = methodInfo.Invoke(invokerObject, parameters);

                return result;
            }
            catch (Exception ex)
            {
                logAction?.Invoke(ex);

                return null;
            }
        }



        /// <summary>
        /// Belirtilen nesne üzerinde belirli bir arayüzün uygulanıp uygulanmadığını kontrol eder.
        /// </summary>
        public static bool ImplementsInterface<TInterface>(object instance)
        {
            return typeof(TInterface).IsAssignableFrom(instance.GetType());
        }

        public static Type? GetTypeFromAssembly(Type sampleTypeInAssembly,
                                            string typeName,
                                            Type? isDerivedFromThisType = null,
                                            Action<Exception>? logAction = null)
        {
            try
            {
                Assembly assembly = sampleTypeInAssembly.Assembly;

                Func<Type, Type, bool> isDerivedFrom = new Func<Type, Type, bool>((t, baseType) =>
                {
                    return t.BaseType?.Name == baseType.Name;
                });

                return assembly.GetTypes().FirstOrDefault(t => t.Name == typeName && (isDerivedFromThisType != null ? isDerivedFrom(t, isDerivedFromThisType) : true));
            }
            catch (Exception ex)
            {
                logAction?.Invoke(ex);

                return null;
            }
        }

        private static void SetNavigationProperties<T>(T tInstance, IEnumerable<dynamic> navigationProperties, PropertyInfo[] properties)
        {
            if (tInstance is null) throw new ArgumentNullException(nameof(tInstance));
            if (navigationProperties is null) throw new ArgumentNullException(nameof(navigationProperties));
            if (properties is null) throw new ArgumentNullException(nameof(properties));

            var groups = navigationProperties.GroupBy(np => np.Table);

            foreach (var group in groups)
            {
                var propertyList = new List<Dictionary<string, object>>
                {
                    group.ToDictionary(
                        a => (string)a.Property, 
                        a => a.Value as object ?? DBNull.Value 
                    )
                };


                var navigationPropertyType = (Type?)GetTypeFromAssembly(typeof(BaseObject<>), group.Key)
                    ?? throw new InvalidOperationException($"Type for navigation property '{group.Key}' not found.");

                var navigationPropertyObject = InvokeMethod(
                    invokerType: typeof(Helper),
                    invokerObject : null,
                    methodName : nameof(ReflectionHelper.Cast),
                    bindingFlags: null,
                    argumentTypesOfMethod: new[] { typeof(List<Dictionary<string, object>>) },
                    genericTypes: new[] { navigationPropertyType },
                    parameters: new object[] { propertyList }
                );

                if (navigationPropertyObject is not IList collection || collection.Count == 0)
                {
                    continue;
                }

                // Navigation property'yi bul ve set et
                var navigationProperty = properties.FirstOrDefault(p => p.PropertyType == navigationPropertyType);

                if (navigationProperty is null)
                {
                    continue;
                }

                navigationProperty.SetValue(tInstance, collection[0]);
            }
        }


        public static MethodInfo? GetMethodInfo(
                                    Type type,
                                    string methodName,
                                    BindingFlags? bindingFlags = null,
                                    Type[]? argumentTypesOfMethod = null,
                                    Type[]? genericTypes = null)
        {
            MethodInfo? methodInfo = null;

            var isGenericMethod = genericTypes != null;

            var getMethodFunc = new Func<Type, MethodInfo?>((type) =>
            {
                var methods = (bindingFlags.HasValue
                                ? type.GetMethods(bindingFlags.Value)
                                : type.GetMethods())
                              .Where(m => m.Name == methodName && m.IsGenericMethod == isGenericMethod);

                if (argumentTypesOfMethod != null)
                {
                    var argumentTypesCount = argumentTypesOfMethod.Length;

                    var argumentTypesNames = argumentTypesOfMethod.Select(t => t.Name).ToList();

                    foreach (var method in methods)
                    {
                        var methodParameters = method.GetParameters();

                        var methodParametersNames = methodParameters
                            .Select(p => !p.ParameterType.IsByRef
                                            ? p.ParameterType.Name
                                            : p.ParameterType.Name.Trim('&'))
                            .ToList();

                        if (methodParametersNames.Intersect(argumentTypesNames).Count() == argumentTypesCount)
                            return method;
                    }

                    return null;
                }
                else
                {
                    return methods.FirstOrDefault();
                }
            });

            methodInfo = getMethodFunc(type) ?? getMethodFunc(type.BaseType!);

            if (isGenericMethod && methodInfo != null)
                methodInfo = methodInfo.MakeGenericMethod(genericTypes!);

            return methodInfo;
        }

        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();

        public static PropertyInfo[] GetCachedProperties(Type type)
        {
            if (PropertyCache.TryGetValue(type, out var cachedProperties))
                return cachedProperties;

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
            PropertyCache[type] = properties;

            return properties;
        }

        public static List<T> Cast<T>(List<Dictionary<string, object?>> rows) where T : class
        {
            var tList = new List<T>();

            var properties = GetCachedProperties(typeof(T));

            Parallel.ForEach(rows, row =>
            {
                var tInstance = CreateInstance<T>();

                foreach (var column in row)
                {
                    var property = properties.FirstOrDefault(p => p.Name == column.Key);
                    if (property != null && property.CanWrite)
                    {
                        try
                        {
                            var value = ConvertValue(column.Value, property.PropertyType);
                            property.SetValue(tInstance, value);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error setting property {property.Name}: {ex.Message}");
                        }
                    }
                }

                if (row.Keys.Any(k => k.Contains(".")))
                {
                    var navigationProperties = row
                        .Where(kvp => kvp.Key.Contains("."))
                        .Select(kvp => new
                        {
                            Table = kvp.Key.Split('.')[0],
                            Property = kvp.Key.Split('.')[1],
                            Value = kvp.Value
                        });

                    SetNavigationProperties(tInstance, navigationProperties, properties);
                }

                lock (tList)
                {
                    if (tInstance != null)
                        tList.Add(tInstance); // Thread-safe ekleme
                }
            });

            return tList;
        }

        private static object? ConvertValue(object? value, Type targetType)
        {
            if (value is null || value == DBNull.Value)
                return null;

            if (targetType.IsEnum)
                return Enum.ToObject(targetType, value);

            if (targetType == typeof(Guid))
                return Guid.TryParse(value.ToString(), out var guid) ? guid : Guid.Empty;

            if (targetType == typeof(string))
                return value.ToString();

            return Convert.ChangeType(value, targetType);
        }
        public static T? CreateInstance<T>(params object[] parameters)
        {
            if (parameters == null)
            {
                Console.WriteLine("Parameters cannot be null");
                return default;
            }

            try
            {
                return (T?)Activator.CreateInstance(typeof(T), parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while creating instance of {typeof(T).FullName}: {ex.Message}");
                return default;
            }
        }

        public static object? CreateInstance(string typeName, params object[] parameters)
        {
            var genericType = GetTypeFromAssembly(typeof(BaseObject<>), typeName);

            if (genericType == null)
                return null;

            var instance = InvokeMethod(invokerType: typeof(Helper),
                                                invokerObject: null,
                                                methodName: nameof(ReflectionHelper.CreateInstance),
                                                bindingFlags: null,
                                                argumentTypesOfMethod: new[]
                                                {
                                                    typeof(object[])
                                                },
                                                genericTypes: new[] { genericType },
                                                parameters: new object[]
                                                {
                                                    parameters
                                                });

            return instance;
        }


    }
}

