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
        void Save(Log log, LogRecordType recordType);

        List<Log> GetRecentLogs();
    }

    public class LogService : ILogService
    {
        private readonly FileService fileService;
        private readonly DatabaseService databaseService;
        private readonly IMemoryCache memoryCache;
        private readonly SessionInformation sessionInformation;

        private const string CACHE_KEY = "RecentLogs";
        private readonly TimeSpan cacheExpiration = TimeSpan.FromHours(1);

        public LogService(
            FileService fileService,
            DatabaseService databaseService,
            IMemoryCache memoryCache,
            SessionInformation sessionInformation)
        {
            this.fileService = fileService;
            this.databaseService = databaseService;
            this.memoryCache = memoryCache;
            this.sessionInformation = sessionInformation;
        }

        public void Save(Log log, LogRecordType recordType)
        {
            log.SetSessionInformation(sessionInformation);

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
                 //   databaseService.Save(log); // Bu metod DatabaseService içinde geliştirilecek
                    break;

                default:
                    throw new InvalidOperationException("Unsupported log record type.");
            }
        }

        private void AddLogToCache(Log log)
        {
            var logs = memoryCache.GetOrCreate(CACHE_KEY, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = cacheExpiration;
                return new List<Log>();
            });

            logs?.Add(log);
            memoryCache.Set(CACHE_KEY, logs, cacheExpiration);
        }

        public List<Log> GetRecentLogs()
        {
            _ = memoryCache.TryGetValue(CACHE_KEY, out List<Log> logs);

            return logs ??  new List<Log>();
        }
    }

}
