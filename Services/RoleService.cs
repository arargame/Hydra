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
            return await _rolePermissionService.GetPermissionsAsync(roleId);
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

