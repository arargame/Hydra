using System;
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

        /// <summary>
        /// Belirtilen tipteki bir metodu çağırır.
        /// </summary>
        public static object? InvokeMethod<T>(T instance, string methodName, params object[] parameters)
        {
            var method = instance?.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
                throw new ArgumentException($"Method '{methodName}' not found on type '{typeof(T)}'.");

            return method.Invoke(instance, parameters);
        }

        /// <summary>
        /// Tipi string olarak belirtilen bir nesne oluşturur.
        /// </summary>
        public static object? CreateInstance(string typeName, params object[] constructorArgs)
        {
            var type = Type.GetType(typeName);
            if (type == null)
                throw new ArgumentException($"Type '{typeName}' not found.");

            return Activator.CreateInstance(type, constructorArgs);
        }

        /// <summary>
        /// Belirtilen nesne üzerinde belirli bir arayüzün uygulanıp uygulanmadığını kontrol eder.
        /// </summary>
        public static bool ImplementsInterface<TInterface>(object instance)
        {
            return typeof(TInterface).IsAssignableFrom(instance.GetType());
        }
    }
}

