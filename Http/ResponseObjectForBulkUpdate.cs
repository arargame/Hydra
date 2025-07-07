using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hydra.Http
{
    public class ResponseObjectForBulkUpdate : ResponseObject
    {
        /// <summary>Başarıyla güncellenen entity ID'leri</summary>
        public List<Guid> UpdatedIds { get; set; } = new();

        /// <summary>Her bir entity için değişen alanlar</summary>
        public Dictionary<Guid, string[]> ModifiedPropertiesById { get; set; } = new();

        /// <summary>Her bir entity için validasyon hataları</summary>
        public Dictionary<Guid, List<ResponseObjectMessage>> ValidationErrorsById { get; set; } = new();

        public ResponseObjectForBulkUpdate AddSuccess(Guid id, string[] modifiedProps)
        {
            if (!UpdatedIds.Contains(id))
                UpdatedIds.Add(id);

            ModifiedPropertiesById[id] = modifiedProps;
            return this;
        }

        public ResponseObjectForBulkUpdate AddValidationErrors(Guid id, List<ResponseObjectMessage> errors)
        {
            if (!ValidationErrorsById.ContainsKey(id))
                ValidationErrorsById[id] = new List<ResponseObjectMessage>();

            ValidationErrorsById[id].AddRange(errors);
            return this;
        }

        public new ResponseObjectForBulkUpdate SetActionName(string? actionName)
        {
            base.SetActionName(actionName);
            return this;
        }

        public new ResponseObjectForBulkUpdate SetId(Guid id)
        {
            base.SetId(id);
            return this;
        }

        public new ResponseObjectForBulkUpdate SetSuccess(bool success)
        {
            base.SetSuccess(success);
            return this;
        }

        public new ResponseObjectForBulkUpdate SetData(object data)
        {
            base.SetData(data);
            return this;
        }

        public new ResponseObjectForBulkUpdate UseDefaultMessages()
        {
            base.UseDefaultMessages();
            return this;
        }

        public new ResponseObjectForBulkUpdate AddExtraMessage(ResponseObjectMessage message)
        {
            base.AddExtraMessage(message);
            return this;
        }

        public new ResponseObjectForBulkUpdate AddExtraMessages(List<ResponseObjectMessage> messages)
        {
            base.AddExtraMessages(messages);
            return this;
        }
    }
}

