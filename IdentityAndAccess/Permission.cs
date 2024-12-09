using Hydra.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.IdentityAndAccess
{
    public class Permission : BaseObject<Permission>
    {
        public ICollection<Role> Roles { get; set; } = new List<Role>();
        public Permission() { }
    }
}
