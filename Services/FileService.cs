using Hydra.FileOperations;
using Hydra.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Services
{
    public class FileService
    {
        public CustomFile CreateCustomFileFromPath(string filePath)
        {
            var fileName = FileHelper.GetFileNameFromPath(filePath);

            return new CustomFile(ReadFileAsBytesFromPath(filePath), fileName);
        }

        public byte[] ReadFileAsBytesFromPath(string filePath)
        {
            return FileHelper.ReadFileAsBytesFromPath(filePath);
        }

        public bool SaveFile(CustomFile customFile, string? path)
        {
            var pathToWrite = path ?? customFile.Path;

            if (pathToWrite == null || customFile.Data == null || customFile.Data.Length == 0)
            {
                throw new ArgumentException("File data or path cannot be null or empty.", customFile.FullName);
            }

            var directory = Path.GetDirectoryName(pathToWrite);

            if (directory == null || !Directory.Exists(directory))
                throw new DirectoryNotFoundException($"Path or directory not found: {pathToWrite}");


            try
            {
                FileHelper.WriteFile(pathToWrite, customFile.Data);
                return true;  
            }
            catch (Exception ex)
            {
                // Hata oluştuğunda log tutabilirsiniz
                Console.WriteLine($"Error writing file: {ex.Message}");
                return false;
            }
        }

        public void SaveFile(string filePath, byte[] content)
        {
            FileHelper.WriteFile(filePath, content);
        }
    }
}
