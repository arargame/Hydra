using Hydra.AccessManagement;
using Hydra.DI;
using Hydra.Http;
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

        public async Task<IResponseObject> GetUsersResponseAsync(Guid permissionId,
            [System.Runtime.CompilerServices.CallerMemberName] string? actionName = null)
        {
            var users = await GetUsersAsync(permissionId);

            return new ResponseObject()
                       .SetActionName(actionName)
                       .SetId(permissionId)
                       .UseDefaultMessages()
                       .SetSuccess(true)
                       .SetData(users);
        }

        public async Task<List<Role>> GetRolesAsync(Guid permissionId)
        {
            return await _rolePermissionService.GetRolesAsync(permissionId);
        }

        public async Task<IResponseObject> GetRolesResponseAsync(Guid permissionId,
            [System.Runtime.CompilerServices.CallerMemberName] string? actionName = null)
        {
            var roles = await GetRolesAsync(permissionId);

            return new ResponseObject()
                       .SetActionName(actionName)
                       .SetId(permissionId)
                       .UseDefaultMessages()
                       .SetSuccess(true)
                       .SetData(roles);
        }
    }
}
