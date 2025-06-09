using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Services.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterAsServiceAttribute : Attribute
    {
        public Type? ServiceInterface { get; set; }
        public RegisterAsServiceAttribute() { }
        public RegisterAsServiceAttribute(Type serviceInterface)
        {
            ServiceInterface = serviceInterface;
        }
    }

}
