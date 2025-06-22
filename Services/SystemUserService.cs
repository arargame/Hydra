using Hydra.DI;
using Hydra.IdentityAndAccess;
using Hydra.Services.Cache;
using Hydra.Services.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hydra.Services
{
    [RegisterAsService(typeof(IService<SystemUser>))]
    public class SystemUserService : Service<SystemUser>
    {
        private readonly RoleSystemUserService _roleSystemUserService;
        private readonly SystemUserPermissionService _systemUserPermissionService;

        public SystemUserService(
            ServiceInjector injector,
            ICacheService<Guid, SystemUser> cache,
            RoleSystemUserService roleSystemUserService,
            SystemUserPermissionService systemUserPermissionService) : base(injector)
        {
            SetCacheService(cache);
            _roleSystemUserService = roleSystemUserService;
            _systemUserPermissionService = systemUserPermissionService;
        }

        public async Task<List<Role>> GetRolesAsync(Guid userId)
        {
            return await _roleSystemUserService.GetRolesAsync(userId);
        }

        public async Task<List<Permission>> GetPermissionsAsync(Guid userId)
        {
            return await _systemUserPermissionService.GetPermissionsAsync(userId);
        }
    }
}

