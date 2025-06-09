using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DAL.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterAsRepositoryAttribute : Attribute
    {
        public Type? RepositoryInterface { get; set; }
        public RegisterAsRepositoryAttribute() { }
        public RegisterAsRepositoryAttribute(Type serviceInterface)
        {
            RepositoryInterface = serviceInterface;
        }
    }

}
