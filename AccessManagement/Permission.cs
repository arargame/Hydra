using Hydra.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.IdentityAndAccess
{
    public enum PermissionType
    {
        ViewBased,
        ControllerActionBased,
        EntityPropertyBased,
        NavMenuBased,
        ComponentBased
    }

    public class Permission : BaseObject<Permission>
    {
        [StringLength(100)]
        public new string? Name { get; set; }
        public PermissionType Type { get; set; }

        public string? Controller { get; set; }

        public string? Action { get; set; }

        public string? Entity { get; set; }

        public string? Property { get; set; }

        public bool AllowAnonymous { get; set; }

        public bool Enabled { get; set; }

        public bool Disabled
        {
            get
            {
                return !Enabled;
            }
        }

        public List<SystemUserPermission> PermissionSystemUsers { get; set; } = new();
        public List<RolePermission> PermissionRoles { get; set; } = new();
        public Permission() { }
    }
}
