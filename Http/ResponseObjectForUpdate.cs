using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Http
{
    public class ResponseObjectForUpdate
    {
        public bool IsSuccess { get; set; }

        public string[]? ModifiedProperties { get; set; } = null;

        public ResponseObjectForUpdate()
        {

        }

        public ResponseObjectForUpdate SetSuccess(bool isSuccess)
        {
            IsSuccess = isSuccess;

            return this;
        }
    }
}
