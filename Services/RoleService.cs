using Hydra.DAL.Core;
using Hydra.DI;
using Hydra.IdentityAndAccess;
using Hydra.Services.Cache;
using Hydra.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Services
{
    [RegisterAsService(typeof(IService<Role>))]
    public class RoleService : Service<Role>
    {
        private readonly ICacheService<Guid, Role> _cache;
        public RoleService(ServiceInjector injector, ICacheService<Guid, Role> cache) : base(injector)
        {
            HasCache = true;

            _cache = cache;
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
