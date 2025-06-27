using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Http
{
    public static class ResponseObjectExtensions
    {
        public static IResponseObject Ok(this ResponseObject response, object data)
            => response.SetSuccess(true).SetData(data);

        public static IResponseObject Fail(this ResponseObject response,string title, string message)
            => response.SetSuccess(false).AddExtraMessage(new ResponseObjectMessage(title, message, false));
    }
}
