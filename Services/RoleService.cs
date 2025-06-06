using Hydra.DAL;
using Hydra.DI;
using Hydra.IdentityAndAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Services
{
    public class RoleService : Service<Role>
    {
        public RoleService(ServiceInjector injector) : base(injector)
        {
            HasCache = true;
        }

        public List<SystemUser> GetUsers(Guid id)
        {
            var userService = new SystemUserService(new ServiceInjector(UnitOfWork));

            var list = userService.SelectWithLinq(j => j.Id == id);

            return list;
        }

        public List<Permission> GetPermissions(Guid id)
        {
            var permissionService = new PermissionService(new ServiceInjector(UnitOfWork));

            var list = permissionService.SelectWithLinq(j => j.Id == id);

            return list;
        }
    }
}
