using System;
using System.Collections.Generic;
using System.Linq;
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
    }

}
