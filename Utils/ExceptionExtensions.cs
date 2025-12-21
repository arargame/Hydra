using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Utils
{
    public static class ExceptionExtensions
    {
        public static string GetFullMessage(this Exception ex)
        {
            var messages = new List<string>();
            var current = ex;

            while (current != null)
            {
                messages.Add(current.Message);
                current = current.InnerException;
            }

            return string.Join(" | ", messages);
        }
    }
}
