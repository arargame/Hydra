using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Core
{
    public enum LogType
    {
        Unspecified,
        Error,      
        Info,       
        Warning
    }

    public enum LogProcessType
    {
        Unspecified,
        Create,    
        Read,    
        Update,    
        Delete    
    }

    public interface ILog
    {
        string? Category { get; set; }

        string? EntityId { get; set; }

        LogType Type { get; set; }

        LogProcessType ProcessType { get; set; }

        ILog SetLogType(LogType logType);
        ILog SetProcessType(LogProcessType processType);
    }

    public class Log : BaseObject<Log>, ILog
    {
        public string? Category { get; set; }

        public string? EntityId { get; set; }

        public LogType Type { get; set; }

        public LogProcessType ProcessType { get; set; }


        public ILog SetLogType(LogType logType)
        {
            Type = logType;

            return this;
        }

        public ILog SetProcessType(LogProcessType logProcessType)
        {
            ProcessType = logProcessType;

            return this;
        }

        public override string ToString()
        {
            return $"({Id})[{Type}][{ProcessType}]{Category}/{Name}/{EntityId}";
        }
    }
}
