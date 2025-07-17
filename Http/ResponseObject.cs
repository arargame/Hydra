using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Http
{
    public interface IResponseObject
    {
        string? ActionName { get; set; }
        Guid Id { get; set; }
        bool Success { get; }
        object? Data { get; set; }
        List<ResponseObjectMessage> Messages { get; set; }
        List<ResponseObjectMessage> GetPositiveMessages { get; }
        List<ResponseObjectMessage> GetNegativeMessages { get; }
        IResponseObject UseDefaultMessages();
        IResponseObject AddExtraMessage(ResponseObjectMessage message);
        IResponseObject AddExtraMessages(List<ResponseObjectMessage> messages);

        IResponseObject SetActionName(string? actionName);
        IResponseObject SetId(Guid id);

        IResponseObject SetSuccess(bool success);

        IResponseObject SetData(object? data);
    }

    public class ResponseObject : IResponseObject
    {
        public string? ActionName { get; set; } = null;
        public Guid Id { get; set; }
        public bool Success { get; set; }
        public object? Data { get; set; } = null;
        public List<ResponseObjectMessage> Messages { get; set; } = new List<ResponseObjectMessage>();

        public List<ResponseObjectMessage> GetPositiveMessages => Messages.Where(m => m.ShowWhenSuccess).ToList();
        public List<ResponseObjectMessage> GetNegativeMessages => Messages.Where(m => !m.ShowWhenSuccess).ToList();

        public IResponseObject UseDefaultMessages()
        {
            Messages.AddRange(ResponseMessageProvider.GetDefaultMessages(ActionName));
            return this;
        }

        public IResponseObject AddExtraMessage(ResponseObjectMessage message)
        {
            Messages.Add(message);
            return this;
        }

        public IResponseObject AddExtraMessages(List<ResponseObjectMessage> messages)
        {
            Messages.AddRange(messages);
            return this;
        }

        public IResponseObject SetActionName(string? actionName)
        {
            ActionName = actionName;
            return this;
        }

        public IResponseObject SetId(Guid id)
        {
            Id = id;
            return this;
        }

        public IResponseObject SetSuccess(bool success)
        {
            Success = success;
            return this;
        }

        public IResponseObject SetData(object? data)
        {
            Data = data;
            return this;
        }
    }


}
