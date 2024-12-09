using Hydra.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.IdentityAndAccess
{
    public class Role : BaseObject<Role>
    {
        public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
        public Role() { }
    }
}
