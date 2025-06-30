using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.AccessManagement
{
    using Hydra.Core;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class RoleSystemUser : BaseObject<RoleSystemUser>
    {
        [Required]
        public Guid RoleId { get; set; }

        [ForeignKey("RoleId")]
        public Role? Role { get; set; } = null;

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public SystemUser? SystemUser { get; set; } = null;

        public RoleSystemUser() { }
    }

}
