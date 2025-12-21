using Hydra.AccessManagement;
using Hydra.Core;
using Hydra.IdentityAndAccess;
using Microsoft.Extensions.Caching.Memory;

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
        private readonly FileLogWriterService _fileLogWriterService;
        private readonly ILogDbWriterService _logDbWriterService;
        private readonly IMemoryCache _memoryCache;
        private readonly ISessionContext _sessionContext;
        private SessionInformation? sessionInformation => _sessionContext.GetCurrent();

        private const string CACHE_KEY = "RecentLogs";
        private readonly TimeSpan cacheExpiration = TimeSpan.FromHours(1);

        public LogService(
            FileLogWriterService fileLogWriterService,
            ILogDbWriterService logDbWriterService,
            IMemoryCache memoryCache,
            ISessionContext sessionContext)
        {
            _fileLogWriterService = fileLogWriterService;
            _logDbWriterService = logDbWriterService;
            _memoryCache = memoryCache;
            _sessionContext = sessionContext;
        }

        public async Task SaveAsync(ILog log, LogRecordType recordType)
        {
            log.SetSessionInformation(sessionInformation);

            // 1. Her halükarda cache'e yaz (son 1 saatlik loglar)
            AddLogToCache(log);

            // 2. Log’u ilgili hedefe yaz
            switch (recordType)
            {
                case LogRecordType.Console:
                    var originalColor = Console.ForegroundColor;
                    switch (log.Type)
                    {
                        case LogType.Error:
                            Console.ForegroundColor = ConsoleColor.Red;
                            break;
                        case LogType.Warning:
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            break;
                        case LogType.Info:
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            break;
                        default:
                            Console.ForegroundColor = ConsoleColor.White;
                            break;
                    }
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{log.Type}] {log.Category}/{log.Name}: {log.Description ?? log.Payload}");
                    Console.ForegroundColor = originalColor;
                    break;

                case LogRecordType.File:
                    await _fileLogWriterService.WriteAsync(log);
                    break;

                case LogRecordType.Database:
                    try
                    {
                        await _logDbWriterService.SaveAsync(log);
                    }
                    catch
                    {
                        await _fileLogWriterService.WriteAsync(log);
                    }
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
