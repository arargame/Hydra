using Hydra.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Hydra.AccessManagement
{

    public class SystemUserPermission : BaseObject<SystemUserPermission>
    {
        [Required]
        public Guid SystemUserId { get; set; }

        [ForeignKey("SystemUserId")]
        public SystemUser? SystemUser { get; set; } = null;

        [Required]
        public Guid PermissionId { get; set; }

        [ForeignKey("PermissionId")]
        public Permission? Permission { get; set; } = null;

        public SystemUserPermission() { }
    }

}
