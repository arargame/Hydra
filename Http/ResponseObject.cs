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

        /// <summary>
        /// Optional HTTP status code carried INSIDE the envelope.
        ///
        /// The envelope contract is unchanged — every endpoint still returns a ResponseObject
        /// and every existing field means exactly what it meant before. This is purely additive:
        /// null (the default) means "nobody set a code", which is how every response built by
        /// older code behaves. When a code IS set, the transport layer mirrors it as the real
        /// HTTP status code, so a denial is both a proper 403 on the wire AND a readable
        /// message in the body. See <see cref="HydraStatusCatalog"/> for code → text.
        /// </summary>
        int? StatusCode { get; set; }

        List<ResponseObjectMessage> GetPositiveMessages { get; }
        List<ResponseObjectMessage> GetNegativeMessages { get; }
        IResponseObject UseDefaultMessages();
        IResponseObject AddExtraMessage(ResponseObjectMessage message);
        IResponseObject AddExtraMessages(List<ResponseObjectMessage> messages);

        IResponseObject SetActionName(string? actionName);
        IResponseObject SetId(Guid id);

        IResponseObject SetSuccess(bool success);

        IResponseObject SetData(object? data);

        IResponseObject SetStatusCode(int? statusCode);
    }

    public class ResponseObject : IResponseObject
    {
        public string? ActionName { get; set; } = null;
        public Guid Id { get; set; }
        public bool Success { get; set; }
        public object? Data { get; set; } = null;
        public List<ResponseObjectMessage> Messages { get; set; } = new List<ResponseObjectMessage>();

        /// <inheritdoc cref="IResponseObject.StatusCode"/>
        public int? StatusCode { get; set; } = null;

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

        public IResponseObject SetActionName([System.Runtime.CompilerServices.CallerMemberName] string? actionName = null)
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

        public IResponseObject SetStatusCode(int? statusCode)
        {
            StatusCode = statusCode;
            return this;
        }
    }

    public class ResponseObject<T> : ResponseObject
    {
        public new T? Data { get; set; }
    }


}
