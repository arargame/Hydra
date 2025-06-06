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
    public class RoleSystemUserService : Service<RoleSystemUser>
    {
        private readonly RoleService RoleService;

        private readonly SystemUserService SystemUserService;

        public RoleSystemUserService(ServiceInjector injector) : base(injector)
        {
            RoleService = new RoleService(injector);

            SystemUserService = new SystemUserService(injector);

            HasCache = true;
        }

        public List<RoleSystemUser> GetRoleSystemUser(Expression<Func<RoleSystemUser, bool>> predicate)
        {
            var roleSystemUsers = new List<RoleSystemUser>();

            if (RoleSystemUserCache.AnyObject(predicate.Compile()))
            {
                roleSystemUsers = RoleSystemUserCache.GetObjectList(predicate.Compile());
            }
            else
            {
                roleSystemUsers = SelectThenCache(filter: predicate);
            }

            return roleSystemUsers;
        }

        public List<Role> GetRoles(Guid userId)
        {
            var roleSystemUsers = GetRoleSystemUser(ru => ru.UserId == userId);

            var roles = new List<Role>();

            foreach (var ru in roleSystemUsers)
            {
                if (RoleCache.AnyObject(r => r.Id == ru.RoleId))
                {
                    roles.Add(RoleCache.GetObjectById(ru.RoleId));
                }
                else
                {
                    var role = RoleService.SelectThenCache(filter: r => r.Id == ru.RoleId).First();

                    if (role != null)
                        roles.Add(role);
                }
            }

            return roles;
        }

        public List<SystemUser> GetUsers(Guid roleId)
        {
            var roleSystemUsers = GetRoleSystemUser(ru => ru.RoleId == roleId);

            var users = new List<SystemUser>();

            foreach (var ru in roleSystemUsers)
            {
                if (SystemUserCache.AnyObject(u => u.Id == ru.UserId))
                {
                    users.Add(SystemUserCache.GetObjectById(ru.UserId));
                }
                else
                {
                    var user = SystemUserService.SelectThenCache(filter: u => u.Id == ru.UserId).First();

                    if (user != null)
                        users.Add(user);
                }
            }

            return users;
        }

        public async Task CheckSystemUserHasPersonelRole()
        {
            var role = RoleService.SelectWithLinq(filter: r => r.Name == "Personel").First();

            var systemUsers = SystemUserService.SelectWithLinq(filter: su => su.IsActive).ToList();

            foreach (var systemUser in systemUsers)
            {
                bool hasRole = Any(filter: rs => rs.RoleId == role.Id && rs.SystemUser.Id == systemUser.Id);

                if (!hasRole)
                {
                    RoleSystemUser roleSystemUser = new RoleSystemUser();

                    roleSystemUser.UserId = systemUser.Id;

                    roleSystemUser.RoleId = role.Id;

                    await CreateOrUpdate(roleSystemUser);
                }
            }
        }
    }
}
