using Hydra.DI;
using Hydra.IdentityAndAccess;
using Hydra.Services.Cache;
using Hydra.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Services
{
    public class RolePermissionService : Service<RolePermission>
    {
        private readonly RoleService RoleService;

        private readonly PermissionService PermissionService;

        private readonly ICacheService<Guid, RolePermission> _cache;

        public RolePermissionService(ServiceInjector injector, ICacheService<Guid, RolePermission> cache) : base(injector)
        {
            RoleService = new RoleService(injector);

            PermissionService = new PermissionService(injector);

            HasCache = true;

            _cache = cache;
        }

        public List<RolePermission> GetRolePermission(Expression<Func<RolePermission, bool>> predicate)
        {
            var rolePermissions = new List<RolePermission>();

            if (RolePermissionCache.AnyObject(predicate.Compile()))
            {
                rolePermissions = RolePermissionCache.GetObjectList(predicate.Compile());
            }
            else
            {
                rolePermissions = SelectThenCache(filter: predicate);
            }

            return rolePermissions;
        }

        public List<Role> GetRoles(Guid permissionId)
        {
            var rolePermissions = GetRolePermission(rp => rp.PermissionId == permissionId);

            var roles = new List<Role>();

            foreach (var rp in rolePermissions)
            {
                if (RoleCache.AnyObject(r => r.Id == rp.RoleId))
                {
                    roles.Add(RoleCache.GetObjectById(rp.RoleId));
                }
                else
                {
                    var role = RoleService.SelectThenCache(p => p.Id == rp.RoleId).First();

                    if (role != null)
                        roles.Add(role);
                }
            }

            return roles;
        }

        public List<Permission> GetPermissions(Guid roleId)
        {
            var rolePermissions = GetRolePermission(rp => rp.RoleId == roleId);

            var permissions = new List<Permission>();

            foreach (var rp in rolePermissions)
            {
                if (PermissionCache.AnyObject(p => p.Id == rp.PermissionId))
                {
                    permissions.Add(PermissionCache.GetObjectById(rp.PermissionId));
                }
                else
                {
                    var permission = PermissionService.SelectThenCache(p => p.Id == rp.PermissionId).First();

                    if (permission != null)
                        permissions.Add(permission);
                }
            }

            return permissions;
        }
    }
}
