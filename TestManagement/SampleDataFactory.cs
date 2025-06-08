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
        public static T CreateSample<T>() where T : BaseObject<T>, new()
        {
            var instance = new T();

            var random = new Random().Next(1000, 9999);
            instance.Name = $"{typeof(T).Name}-{random}";
            instance.Description = $"Description: {Guid.NewGuid()}";

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
