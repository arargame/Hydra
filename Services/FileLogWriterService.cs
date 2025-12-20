using Hydra.Core;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hydra.Services
{
    public class FileLogWriterService
    {
        private readonly string _basePath;

        public FileLogWriterService(IConfiguration config)
        {
            _basePath = config["Logging:File:Path"] ?? "C:/HydraLogs";
        }

        public virtual async Task WriteAsync(ILog log)
        {
            try
            {
                if (!Directory.Exists(_basePath))
                {
                    Directory.CreateDirectory(_basePath);
                }

                var filePath = Path.Combine(
                    _basePath,
                    $"{DateTime.UtcNow:yyyy-MM-dd}.log"
                );

                var line = JsonSerializer.Serialize(log);
                await File.AppendAllTextAsync(filePath, line + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // Fallback fails silently or maybe writes to console to avoid app crash
                Console.WriteLine($"Failed to write to log file: {ex.Message}");
            }
        }
    }
}
