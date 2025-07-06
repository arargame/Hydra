﻿using Hydra.AccessManagement;
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
            string? entityId = null,
            LogProcessType processType = LogProcessType.Unspecified,
            SessionInformation? sessionInformation = null)
                => new Log(category, name, description, LogType.Info, entityId, processType, sessionInformation);

        public static Log Error(string description, string? entityId = null, LogProcessType processType = LogProcessType.Unspecified, SessionInformation? sessionInformation = null)
            => new Log(description, LogType.Error, entityId, processType, sessionInformation, 2);

        public static Log Warning(string? category,
            string? name,
            string? description,
            LogType logType,
            string? entityId = null,
            LogProcessType processType = LogProcessType.Unspecified,
            SessionInformation? sessionInformation = null)
                => new Log(category, name, description, LogType.Warning, entityId, processType, sessionInformation);
    }

}
