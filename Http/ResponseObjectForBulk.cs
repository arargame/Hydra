using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Http
{
    public class ResponseObjectForBulk : IResponseObject
    {
        public string? ActionName { get; set; } = null;
        public Guid Id { get; set; }
        public bool Success => !ResponseObjects.Any(ro => !ro.Success);
        public object? Data { get; set; } = null;
        public List<ResponseObjectMessage> Messages { get; set; } = new List<ResponseObjectMessage>();
        public List<ResponseObject> ResponseObjects { get; set; } = new List<ResponseObject>();

        public List<ResponseObjectMessage> GetPositiveMessages => ResponseObjects.SelectMany(ro => ro.GetPositiveMessages).ToList();
        public List<ResponseObjectMessage> GetNegativeMessages => ResponseObjects.SelectMany(ro => ro.GetNegativeMessages).ToList();

        public IResponseObject UseDefaultMessages()
        {
            Messages.AddRange(MessageProvider.GetDefaultMessages(ActionName));
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

        public ResponseObjectForBulk SetResponseObjects(List<ResponseObject> responseObjects)
        {
            ResponseObjects = responseObjects;
            return this;
        }

        public ResponseObjectForBulk SetActionName(string actionName)
        {
            ActionName = actionName;
            return this;
        }

        public ResponseObjectForBulk SetId(Guid id)
        {
            Id = id;
            return this;
        }
    }

}
