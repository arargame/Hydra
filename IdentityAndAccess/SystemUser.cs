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

        public string? PhoneNumber { get; set; } = null;

        public bool PhoneNumberConfirmed { get; set; }

        [NotMapped]
        public bool IsAuthenticated { get; set; }

        public Guid? PasswordResetValidationToken { get; set; } = null;

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
