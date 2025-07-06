using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Http
{
    public class ResponseObjectForUpdate : ResponseObject
    {
        public string[]? ModifiedProperties { get; set; } = null;

        public new ResponseObjectForUpdate SetSuccess(bool isSuccess)
        {
            base.SetSuccess(isSuccess);
            return this;
        }

        public ResponseObjectForUpdate AddModifiedProperties(string[] props)
        {
            ModifiedProperties = props;
            return this;
        }
    }

}
