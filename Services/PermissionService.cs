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
        private readonly ServiceInjector _injector;
        // Dependencies removed from constructor to break circular reference

        public PermissionService(
            ServiceInjector injector,
            IQueryableCacheService<Guid, Permission> cache) : base(injector)
        {
            _injector = injector;
            SetCacheService(cache);
        }


        public async Task<List<SystemUser>> GetUsersAsync(Guid permissionId)
        {
            var systemUserPermissionService = _injector.ResolveLazy<SystemUserPermissionService>().Value;
            return await systemUserPermissionService.GetUsersAsync(permissionId);
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
            var rolePermissionService = _injector.ResolveLazy<RolePermissionService>().Value;
            return await rolePermissionService.GetRolesAsync(permissionId);
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
