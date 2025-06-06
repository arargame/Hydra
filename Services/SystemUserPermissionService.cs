using Hydra.DI;
using Hydra.IdentityAndAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Services
{
    public class SystemUserPermissionService : Service<SystemUserPermission>
    {
        private readonly SystemUserService SystemUserService;

        private readonly PermissionService PermissionService;

        public SystemUserPermissionService(ServiceInjector injector) : base(injector)
        {
            SystemUserService = new SystemUserService(injector);

            PermissionService = new PermissionService(injector);

            HasCache = true;
        }

        public List<SystemUserPermission> GetSystemUserPermission(Expression<Func<SystemUserPermission, bool>> predicate)
        {
            var systemUserPermissions = new List<SystemUserPermission>();

            if (SystemUserPermissionCache.AnyObject(predicate.Compile()))
            {
                systemUserPermissions = SystemUserPermissionCache.GetObjectList(predicate.Compile());
            }
            else
            {
                systemUserPermissions = SelectThenCache(filter: predicate);
            }

            return systemUserPermissions;
        }

        public List<SystemUser> GetUsers(Guid permissionId)
        {
            var systemUserPermissions = GetSystemUserPermission(up => up.PermissionId == permissionId);

            var users = new List<SystemUser>();

            foreach (var up in systemUserPermissions)
            {
                if (SystemUserCache.AnyObject(u => u.Id == up.UserId))
                {
                    users.Add(SystemUserCache.GetObjectById(up.UserId));
                }
                else
                {
                    var user = SystemUserService.SelectThenCache(filter: u => u.Id == up.UserId).First();

                    if (user != null)
                        users.Add(user);
                }
            }

            return users;
        }

        public List<Permission> GetPermissions(Guid userId)
        {
            var systemUserPermissions = GetSystemUserPermission(up => up.UserId == userId);

            var permissions = new List<Permission>();

            foreach (var up in systemUserPermissions)
            {
                if (PermissionCache.AnyObject(p => p.Id == up.PermissionId))
                {
                    permissions.Add(PermissionCache.GetObjectById(up.PermissionId));
                }
                else
                {
                    var permission = PermissionService.SelectThenCache(filter: p => p.Id == up.PermissionId).First();

                    if (permission != null)
                        permissions.Add(permission);
                }
            }

            return permissions;
        }
    }
}
