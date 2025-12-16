using Hydra.AccessManagement;
using Hydra.DI;
using Hydra.Http;
using Hydra.IdentityAndAccess;
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
        private readonly ServiceInjector _injector;

        public RoleService(
            ServiceInjector injector,
            IQueryableCacheService<Guid, Role> cache) : base(injector)
        {
            _injector = injector;
            SetCacheService(cache);
        }

        public async Task<List<SystemUser>> GetUsersAsync(Guid roleId)
        {
            var roleSystemUserService = _injector.ResolveLazy<RoleSystemUserService>().Value;
            return await roleSystemUserService.GetUsersAsync(roleId);
        }

        public async Task<IResponseObject> GetUsersResponseAsync(Guid roleId, [System.Runtime.CompilerServices.CallerMemberName] string? actionName = null)
        {
            var users = await GetUsersAsync(roleId);

            return new ResponseObject()
                       .SetActionName(actionName)
                       .SetId(roleId)
                       .UseDefaultMessages()
                       .SetSuccess(true)
                       .SetData(users);
        }

        public async Task<List<Permission>> GetPermissionsAsync(Guid roleId)
        {
            var rolePermissionService = _injector.ResolveLazy<RolePermissionService>().Value;
            return await rolePermissionService.GetPermissionsAsync(roleId);
        }

        public async Task<IResponseObject> GetPermissionsResponseAsync(Guid roleId, [System.Runtime.CompilerServices.CallerMemberName] string? actionName = null)
        {
            var permissions = await GetPermissionsAsync(roleId);

            return new ResponseObject()
                       .SetActionName(actionName)
                       .SetId(roleId)
                       .UseDefaultMessages()
                       .SetSuccess(true)
                       .SetData(permissions);
        }
    }
}

