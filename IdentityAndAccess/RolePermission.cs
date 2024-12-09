using Hydra.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.IdentityAndAccess
{
    public class RolePermission : BaseObject<RolePermission>
    {

        [Required]
        public Guid RoleId { get; set; }

        [ForeignKey("RoleId")]
        public Role? Role { get; set; } = null;

        [Required]
        public Guid PermissionId { get; set; }

        [ForeignKey("PermissionId")]
        public Permission? Permission { get; set; } = null;

        public RolePermission() { }
    }
}
