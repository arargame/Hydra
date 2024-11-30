using Hydra.Core;
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

    public class LogService
    {
        private readonly FileService fileService;

        private readonly DatabaseService databaseService;

        public LogService(FileService fileService, DatabaseService databaseService)
        {
            this.fileService = fileService;

            this.databaseService = databaseService;
        }

        public void Save(Log log,LogRecordType recordType)
        {
            switch (recordType)
            {
                case LogRecordType.Console:

                    Console.WriteLine(log.ToString());

                    break;

                case LogRecordType.File:

                    //fileService.

                    break;

                case LogRecordType.Database:

                   // databaseService.

                    break;

                default:
                    throw new InvalidOperationException("Unsupported log record type.");
            }
        }
    }
}
