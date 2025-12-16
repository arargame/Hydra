using Hydra.DI;
using Hydra.AccessManagement;
using Hydra.Services.Cache;
using Hydra.Services.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hydra.IdentityAndAccess;

namespace Hydra.Services
{
    [RegisterAsService(typeof(IService<SystemUser>))]
    public class SystemUserService : Service<SystemUser>
    {
        private readonly ServiceInjector _injector;

        public SystemUserService(
            ServiceInjector injector,
            IQueryableCacheService<Guid, SystemUser> cache) : base(injector)
        {
            _injector = injector;
            SetCacheService(cache);
        }

        public async Task<List<Role>> GetRolesAsync(Guid userId)
        {
            var roleSystemUserService = _injector.ResolveLazy<RoleSystemUserService>().Value;
            return await roleSystemUserService.GetRolesAsync(userId);
        }

        public async Task<List<Permission>> GetPermissionsAsync(Guid userId)
        {
            var systemUserPermissionService = _injector.ResolveLazy<SystemUserPermissionService>().Value;
            return await systemUserPermissionService.GetPermissionsAsync(userId);
        }
    }
}

