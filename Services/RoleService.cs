using Hydra.DI;
using Hydra.AccessManagement;
using Hydra.Services.Cache;
using Hydra.Services.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hydra.Services
{
    [RegisterAsService(typeof(IService<Role>))]
    public class RoleService : Service<Role>
    {
        private readonly RoleSystemUserService _roleSystemUserService;
        private readonly RolePermissionService _rolePermissionService;

        public RoleService(
            ServiceInjector injector,
            IQueryableCacheService<Guid, Role> cache,
            RoleSystemUserService roleSystemUserService,
            RolePermissionService rolePermissionService) : base(injector)
        {
            SetCacheService(cache);
            _roleSystemUserService = roleSystemUserService;
            _rolePermissionService = rolePermissionService;
        }

        public async Task<List<SystemUser>> GetUsersAsync(Guid roleId)
        {
            return await _roleSystemUserService.GetUsersAsync(roleId);
        }

        public async Task<List<Permission>> GetPermissionsAsync(Guid roleId)
        {
            return await _rolePermissionService.GetPermissionsAsync(roleId);
        }
    }
}

