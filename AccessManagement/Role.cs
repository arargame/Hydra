using Hydra.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.AccessManagement
{
    public class Role : BaseObject<Role>
    {
        public List<RoleSystemUser> RoleSystemUsers { get; set; } = new();

        public List<RolePermission> RolePermissions { get; set; } = new();
        public Role() { }
    }
}
