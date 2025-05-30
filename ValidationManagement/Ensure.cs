using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.ValidationManagement
{
    namespace Hydra.ValidationManagement
    {
        public static class Ensure
        {
            public static T NotNull<T>(T? value, string paramName) where T : class
            {
                if (value == null)
                    throw new ArgumentNullException(paramName);

                return value;
            }

            public static string NotNullOrWhiteSpace(string? value, string paramName)
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException($"'{paramName}' cannot be null or whitespace.", paramName);

                return value!;
            }

            public static void IsTrue(bool condition, string message)
            {
                if (!condition)
                    throw new InvalidOperationException(message);
            }

            public static void IsFalse(bool condition, string message)
            {
                if (condition)
                    throw new InvalidOperationException(message);
            }

            public static int InRange(int value, int min, int max, string paramName)
            {
                if (value < min || value > max)
                    throw new ArgumentOutOfRangeException(paramName, $"Value must be between {min} and {max}");

                return value;
            }
        }
    }

}
