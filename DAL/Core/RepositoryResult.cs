using Hydra.Core;
using Hydra.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DAL.Core
{
    public class RepositoryResult
    {
        public bool IsSuccess { get; set; }
        public List<ResponseObjectMessage> Messages { get; set; } = new();
        public List<ILog> Logs { get; set; } = new ();

        public RepositoryResult() { }

        public RepositoryResult AddLog(ILog log)
        {
            Logs.Add(log);

            return this;
        }


        public RepositoryResult AddMessage(string title, string text, bool showWhenSuccess = false)
        {
            Messages.Add(new ResponseObjectMessage(title, text, showWhenSuccess));

            return SetSuccess(showWhenSuccess);
        }

        public RepositoryResult AddErrorMessage(string title, string text)
        {
            return AddMessage(title, text, showWhenSuccess: false);
        }

        public RepositoryResult AddSuccessMessage(string title, string text)
        {
            return AddMessage(title, text, showWhenSuccess: true);
        }

        public List<ILog> ConsumeLogs()
        {
            var logList = new List<ILog>(Logs);

            Logs.Clear();

            return logList;
        }

        public List<ResponseObjectMessage> ConsumeMessages()
        {
            var msgList = new List<ResponseObjectMessage>(Messages);
            Messages.Clear();
            return msgList;
        }

        public RepositoryResult SetSuccess(bool isSuccess)
        {
            IsSuccess = isSuccess;

            return this;
        }
    }
}
