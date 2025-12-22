using Hydra.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.TestManagement
{
    public static class SampleDataFactory
    {
        private static readonly Random _random = new Random();

        public static T CreateSample<T>() where T : BaseObject<T>, new()
        {
            var instance = new T();
            var properties = Hydra.Utils.ReflectionHelper.GetCachedProperties(typeof(T));

            foreach (var prop in properties)
            {
                if (!prop.CanWrite || prop.Name == "Id" || prop.Name == "CreatedAt" || prop.Name == "UpdatedAt") continue;

                if (prop.PropertyType == typeof(string))
                {
                    prop.SetValue(instance, $"{prop.Name}-{_random.Next(100,999)}");
                }
                else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?))
                {
                    prop.SetValue(instance, _random.Next(1, 100));
                }
                else if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                {
                    prop.SetValue(instance, DateTime.UtcNow);
                }
                else if (prop.PropertyType == typeof(bool) || prop.PropertyType == typeof(bool?))
                {
                    prop.SetValue(instance, _random.Next(0, 2) == 1);
                }
            }

            return instance;
        }

        public static List<T> CreateSamples<T>(int count) where T : BaseObject<T>, new()
        {
            var list = new List<T>();
            for (int i = 0; i < count; i++)
            {
                list.Add(CreateSample<T>());
            }

            return list;
        }
    }

}
