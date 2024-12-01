using Hydra.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Services
{
    public class FileService
    {
        public byte[] ReadFile(string filePath)
        {
            return FileHelper.ReadFileAsBytesFromPath(filePath);
        }

        public void SaveFile(string filePath, byte[] content)
        {
            FileHelper.WriteFile(filePath, content);
        }
    }
}
