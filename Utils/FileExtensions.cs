using Hydra.FileOperations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Utils
{
    public static class FileExtensions
    {
        // Resim dosya uzantıları
        public static string[] ImageFileExtensions = new string[]
        {
        "png", "jpeg", "jpg", "gif", "bmp", "tiff", "ico", "heic", "svg"
        };

        // Ses dosya uzantıları
        public static string[] AudioFileExtensions = new string[]
        {
        "wav", "mp3", "flac", "aac", "m4a", "ogg"
        };

        // Video dosya uzantıları
        public static string[] VideoFileExtensions = new string[]
        {
        "mp4", "webm", "ogg", "avi", "mov", "mkv", "wmv"
        };

        // Doküman dosya uzantıları
        public static string[] DocumentFileExtensions = new string[]
        {
        "pdf", "txt", "xlsx", "xls", "docx", "doc", "ppt", "pptx", "odt", "ods"
        };

        // Sıkıştırılmış dosya uzantıları
        public static string[] ArchiveFileExtensions = new string[]
        {
        "zip", "rar", "7z", "tar", "gz"
        };

        // Kod dosyaları için uzantılar
        public static string[] CodeFileExtensions = new string[]
        {
        "cs", "js", "ts", "py", "html", "css", "java"
        };

        public static FileCategory GetFileCategory(string? extension)
        {
            extension = extension?.ToLower() ?? "";

            if (ImageFileExtensions.Contains(extension))
                return FileCategory.Image;

            if (AudioFileExtensions.Contains(extension))
                return FileCategory.Audio;

            if (VideoFileExtensions.Contains(extension))
                return FileCategory.Video;

            if (DocumentFileExtensions.Contains(extension))
                return FileCategory.Document;

            return FileCategory.Unsupported;
        }
    }

}
