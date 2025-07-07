using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    namespace Hydra.Http
    {
        public class ResponseObjectForUpdate : ResponseObject
        {
            public List<string> ModifiedProperties { get; set; } = new();

            public ResponseObjectForUpdate AddModifiedProperty(string propertyName)
            {
                if (!string.IsNullOrWhiteSpace(propertyName))
                    ModifiedProperties.Add(propertyName);

                return this;
            }

            public ResponseObjectForUpdate AddModifiedProperties(IEnumerable<string> props)
            {
                if (props != null)
                    ModifiedProperties.AddRange(props.Where(p => !string.IsNullOrWhiteSpace(p)));

                return this;
            }

            // Override fluent methods from base ResponseObject
            public new ResponseObjectForUpdate SetSuccess(bool isSuccess)
            {
                base.SetSuccess(isSuccess);
                return this;
            }

            public new ResponseObjectForUpdate SetActionName(string? actionName)
            {
                base.SetActionName(actionName);
                return this;
            }

            public new ResponseObjectForUpdate SetId(Guid id)
            {
                base.SetId(id);
                return this;
            }

            public new ResponseObjectForUpdate SetData(object data)
            {
                base.SetData(data);
                return this;
            }

            public new ResponseObjectForUpdate UseDefaultMessages()
            {
                base.UseDefaultMessages();
                return this;
            }

            public new ResponseObjectForUpdate AddExtraMessage(ResponseObjectMessage message)
            {
                base.AddExtraMessage(message);
                return this;
            }

            public new ResponseObjectForUpdate AddExtraMessages(List<ResponseObjectMessage> messages)
            {
                base.AddExtraMessages(messages);
                return this;
            }
        }
    }


}
