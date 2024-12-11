using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Utils
{
    public static class EnumHelper
    {
        public static int ToInt<TEnum>(TEnum enumValue) where TEnum : Enum
        {
            return Convert.ToInt32(enumValue);
        }

        public static TEnum ToEnum<TEnum>(int value) where TEnum : Enum
        {
            return (TEnum)Enum.ToObject(typeof(TEnum), value);
        }

        public static string ToString<TEnum>(TEnum enumValue) where TEnum : Enum
        {
            return enumValue.ToString();
        }

        public static TEnum ToEnumFromString<TEnum>(string value) where TEnum : Enum
        {
            return (TEnum)Enum.Parse(typeof(TEnum), value, true); 
        }

        public static string GetEnumAttributeString<TEnum>(TEnum enumValue,bool isDisplay = true) where TEnum : Enum
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());

            if (isDisplay)
            {
                var displayAttribute = field?.GetCustomAttribute<DisplayAttribute>();

                return displayAttribute?.Name ?? enumValue.ToString();
            }
            else
            {
                var descriptionAttribute = field?.GetCustomAttribute<DescriptionAttribute>();

                return descriptionAttribute?.Description ?? enumValue.ToString();
            }
        }


        public static int ToIntFromString(string value)
        {
            return int.TryParse(value, out int result) ? result : 0;
        }

        public static Dictionary<string, int> EnumInformation(Type sampleTypeInAssembly,
                                                      string enumTypeName,
                                                      bool getDisplayName = false,
                                                      bool getDescription = false)
        {
            var collection = new Dictionary<string, int>();

            var enumType = ReflectionHelper.GetTypeFromAssembly(sampleTypeInAssembly, enumTypeName);

            if (enumType == null || !enumType.IsEnum)
            {
                throw new ArgumentException("The provided type is not a valid Enum type.");
            }

            var enumValues = enumType.GetEnumValues();

            foreach (var enumValue in enumValues)
            {
                if (enumValue == null)
                {
                    continue; 
                }

                var key = "";
                var value = (int)enumValue;

                if (getDisplayName)
                {
                    key = GetEnumAttributeString(enumValue: (Enum)enumValue, isDisplay: true);
                }
                else if (getDescription)
                {
                    key = GetEnumAttributeString(enumValue: (Enum)enumValue, isDisplay: false); 
                }
                else
                {
                    key = enumValue.ToString();
                }

                collection.Add(key, value);
            }

            return collection;
        }

    }

}
