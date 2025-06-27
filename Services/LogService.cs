using Hydra.Core;
using Hydra.IdentityAndAccess;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Services
{
    public enum LogRecordType
    {
        Console,
        File,
        Database
    }

    public interface ILogService
    {
        Task SaveAsync(ILog log, LogRecordType recordType);

        List<ILog> GetRecentLogs();
    }

    public class LogService : ILogService
    {
        private readonly FileService _fileService;
        private readonly ILogDbWriterService _logDbWriterService;
        private readonly IMemoryCache _memoryCache;
        private readonly SessionInformation _sessionInformation;

        private const string CACHE_KEY = "RecentLogs";
        private readonly TimeSpan cacheExpiration = TimeSpan.FromHours(1);

        public LogService(
            FileService fileService,
            ILogDbWriterService logDbWriterService,
            IMemoryCache memoryCache,
            SessionInformation sessionInformation)
        {
            _fileService = fileService;
            _logDbWriterService = logDbWriterService;
            _memoryCache = memoryCache;
            _sessionInformation = sessionInformation;
        }

        public async Task SaveAsync(ILog log, LogRecordType recordType)
        {
            log.SetSessionInformation(_sessionInformation);

            // 1. Her halükarda cache'e yaz (son 1 saatlik loglar)
            AddLogToCache(log);

            // 2. Log’u ilgili hedefe yaz
            switch (recordType)
            {
                case LogRecordType.Console:
                    Console.WriteLine(log.ToString());
                    break;

                case LogRecordType.File:
                   // fileService.Save(log); // Bu metod FileService içinde geliştirilecek
                    break;

                case LogRecordType.Database:
                    await _logDbWriterService.SaveAsync(log);
                    break;

                default:
                    throw new InvalidOperationException("Unsupported log record type.");
            }
        }

        private void AddLogToCache(ILog log)
        {
            var logs = _memoryCache.GetOrCreate(CACHE_KEY, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = cacheExpiration;
                return new List<ILog>();
            });

            logs?.Add(log);
            _memoryCache.Set(CACHE_KEY, logs, cacheExpiration);
        }

        public List<ILog> GetRecentLogs()
        {
            _ = _memoryCache.TryGetValue(CACHE_KEY, out List<ILog> logs);

            return logs ??  new List<ILog>();
        }
    }

}
