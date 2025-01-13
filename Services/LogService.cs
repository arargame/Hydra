using Hydra.Core;
using Hydra.IdentityAndAccess;
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

        private readonly SessionInformation sessionInformation;
        public LogService(FileService fileService, DatabaseService databaseService,SessionInformation sessionInformation)
        {
            this.fileService = fileService;

            this.databaseService = databaseService;

            this.sessionInformation = sessionInformation;
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
