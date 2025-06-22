using Hydra.DI;
using Hydra.IdentityAndAccess;
using Hydra.Services.Cache;
using Hydra.Services.Core;

namespace Hydra.Services
{
    [RegisterAsService(typeof(IService<Permission>))]
    public class PermissionService : Service<Permission>
    {
        private readonly SystemUserPermissionService _systemUserPermissionService;
        private readonly RolePermissionService _rolePermissionService;

        public PermissionService(
            ServiceInjector injector,
            IQueryableCacheService<Guid, Permission> cache,
            SystemUserPermissionService systemUserPermissionService,
            RolePermissionService rolePermissionService) : base(injector)
        {
            SetCacheService(cache);
            _systemUserPermissionService = systemUserPermissionService;
            _rolePermissionService = rolePermissionService;
        }


        public async Task<List<SystemUser>> GetUsersAsync(Guid permissionId)
        {
            return await _systemUserPermissionService.GetUsersAsync(permissionId);
        }


        public async Task<List<Role>> GetRolesAsync(Guid permissionId)
        {
            return await _rolePermissionService.GetRolesAsync(permissionId);
        }
    }
}
