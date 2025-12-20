using Hydra.AccessManagement;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
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
        CreateBulk,
        Read,    
        Update,
        UpdateBulk,
        Delete,
        DeleteBulk,
        Login,
        Logout
    }

    public interface ILog : IBaseObject<ILog>
    {
        string? Category { get; set; }

        string? EntityName {  get; set; }

        string? EntityId { get; set; }

        LogType Type { get; set; }

        LogProcessType ProcessType { get; set; }

        Guid? SessionInformationId {  get; set; }

        ILog SetLogType(LogType logType);
        ILog SetProcessType(LogProcessType processType);

        ILog SetCategory(string? category);

        ILog SetEntityId(string? entityId);

        ILog SetEntityName(string? entityName);


        ILog SetSessionInformation(SessionInformation? sessionInformation);
        ILog SetPayload(string? payload);

        string? Payload { get; set; }
    }

    [Index(nameof(Log.EntityId))]
    [Index(nameof(Log.SessionInformationId))]
    [Index(nameof(Log.Category))]
    public class Log : BaseObject<Log>, ILog
    {
        public string? Category { get; set; }

        public string? EntityName { get; set; }

        public string? EntityId { get; set; }

        public LogType Type { get; set; }

        public LogProcessType ProcessType { get; set; }

        public string? Payload { get; set; }

        public Guid? SessionInformationId { get; set; }

        [ForeignKey("SessionInformationId")]
        public SessionInformation? SessionInformation { get; set; } = null;

        public Log() { }

        public Log(string? category,
            string? name,
            string? description,
            LogType logType,
            string ? entityName = null,
            string? entityId = null,
            LogProcessType processType = LogProcessType.Unspecified,
            SessionInformation? sessionInformation = null)
        {
            //Initialize();

            SetCategory(category);

            SetName(name);

            SetDescription(description);

            SetLogType(logType);

            SetEntityId(entityId);

            SetProcessType(processType);

            SetSessionInformation(sessionInformation);
        }

        public Log(string description,
            LogType logType = LogType.Error,
            string? entityName = null,
            string? entityId = null,
            LogProcessType processType = LogProcessType.Unspecified,
            SessionInformation? sessionInformation = null,
            int frameIndex = 1) : this(null, null, description, logType, entityName, entityId, processType, sessionInformation)
        {
            var methodBase = new StackTrace().GetFrame(frameIndex)?.GetMethod();

            SetCategory(methodBase?.DeclaringType?.Name);

            SetName(methodBase?.Name);
        }

        public override void Initialize()
        {
            Id = Guid.NewGuid();

            AddedDate = DateTime.Now;

            ModifiedDate = DateTime.Now;
        }


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

        public ILog SetCategory(string? category)
        {
            Category = category;

            return this;
        }

        public ILog SetEntityId(string? entityId)
        {
            EntityId = entityId;

            return this;
        }

        public ILog SetEntityName(string? entityName)
        {
            EntityName = entityName;

            return this;
        }

        public ILog SetSessionInformation(SessionInformation? sessionInformation)
        {
            SessionInformation = sessionInformation;

            return this;
        }

        public ILog SetPayload(string? payload)
        {
            Payload = payload;
            return this;
        }
    }
}
