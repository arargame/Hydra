using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Utils
{
    public static class FileHelper
    {
        public static byte[] ReadFileAsBytesFromPath(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            return File.ReadAllBytes(filePath);
        }

        public static string GetFileNameFromPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path cannot be null or empty", nameof(path));
            }

            return Path.GetFileName(path);
        }

        public static void WriteFile(string filePath, byte[] content)
        {
            File.WriteAllBytes(filePath, content);
        }

        public static string? GetFileNameWithoutExtension(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        public static string GetFileExtensionWithoutDot(string filePath)
        {
            string extension = Path.GetExtension(filePath);

            return extension.TrimStart('.');
        }
    }
}
