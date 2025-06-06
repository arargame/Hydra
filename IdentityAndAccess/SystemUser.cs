using Hydra.AddressManagement;
using Hydra.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.IdentityAndAccess
{
    public class SystemUser : BaseObject<SystemUser>
    {
        [Required]
        public string? Email { get; set; } = null;

        public bool EmailConfirmed { get; set; }

        public string? PasswordHash { get; set; } = null;

        public string? NickName { get; set; } = null;

        public Guid? PhoneNumberId { get; set; }
        public PhoneNumber? PhoneNumber { get; set; } = null;

        public bool PhoneNumberConfirmed { get; set; }

        [NotMapped]
        public bool IsAuthenticated { get; set; }

        public Guid? PasswordResetValidationToken { get; set; } = null;

        public ICollection<Role> Roles { get; set; } = new List<Role>();

        public List<RoleSystemUser> SystemUserRoles { get; set; } = new();

        public List<SystemUserPermission> SystemUserPermissions { get; set; } = new();

        public List<SessionInformation> SessionInformations { get; set; } = new();

        public SystemUser() { }

        public override string UniqueProperty
        {
            get
            {
                return Email ?? base.UniqueProperty;  
            }
        }
    }
}
