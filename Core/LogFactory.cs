using Hydra.AccessManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Core
{
    public static class LogFactory
    {
        public static Log Info(string? category,
            string? name,
            string? description,
            string? entityName=null,
            string? entityId = null,
            LogProcessType processType = LogProcessType.Unspecified,
            SessionInformation? sessionInformation = null)
                => new Log(category, name, description, LogType.Info, entityName,entityId, processType, sessionInformation);

        public static Log Error(string description, string? entityName = null, string? entityId = null, LogProcessType processType = LogProcessType.Unspecified, SessionInformation? sessionInformation = null)
            => new Log(description, LogType.Error, entityName, entityId, processType, sessionInformation, 2);

        public static Log Warning(string? category,
            string? name,
            string? description,
            LogType logType,
            string? entityName = null,
            string? entityId = null,
            LogProcessType processType = LogProcessType.Unspecified,
            SessionInformation? sessionInformation = null)
                => new Log(category, name, description, LogType.Warning,entityName, entityId, processType, sessionInformation);

        public static ILog SetPayload(this Log log, string payload)
        {
            return log.SetPayload(payload);
        }
    }

}
