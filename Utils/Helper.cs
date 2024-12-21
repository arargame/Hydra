using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Utils
{
    public static class Helper
    {
        public static string? ByteArrayToString(byte[]? byteArray)
        {
            if (byteArray == null)
            {
                return null;
            }

            return Encoding.UTF8.GetString(byteArray);
        }

        public static string? ConvertToBase64String(byte[]? byteArray)
        {
            if (byteArray == null)
            {
                return null;
            }

            return Convert.ToBase64String(byteArray);
        }

        public static char GenerateUnusedCharacterInAWord(string word)
        {
            var alias = ' ';

            var allEnglishLettersInAWord = "The quick brown fox jumps over the lazy dog".Replace(" ", "");

            do
            {
                var randomIndex = new Random().Next(0, allEnglishLettersInAWord.Length);

                alias = allEnglishLettersInAWord[randomIndex];

            } while (word.Contains(alias));

            return alias;
        }

        public static string GetRandomAlphanumericString(int length)
        {
            const string alphanumericCharacters =
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                "abcdefghijklmnopqrstuvwxyz" +
                "0123456789";
            return GetRandomString(length, alphanumericCharacters);
        }

        public static string GetRandomString(int length, IEnumerable<char> characterSet)
        {
            if (length < 0)
                throw new ArgumentException("length must not be negative", nameof(length));
            if (length > int.MaxValue / 8) 
                throw new ArgumentException("length is too big", nameof(length));
            if (characterSet == null)
                throw new ArgumentNullException(nameof(characterSet));

            var characterArray = characterSet.Distinct().ToArray();
            if (characterArray.Length == 0)
                throw new ArgumentException("characterSet must not be empty", nameof(characterSet));


            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[length];
                rng.GetBytes(bytes); 

                var result = new char[length];
                for (int i = 0; i < length; i++)
                {
                    ulong value = BitConverter.ToUInt64(bytes, i * 8);
                    result[i] = characterArray[value % (uint)characterArray.Length];
                }

                return new string(result);
            }
        }


        public static Guid? GetNullableGuid(object objectToConvert)
        {
            if (objectToConvert == null)
                return null;

            if (Guid.TryParse(objectToConvert.ToString(), out Guid result) && result != Guid.Empty)
                return result;

            return null;
        }

    }

}
