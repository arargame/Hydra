using Hydra.DI;
using Hydra.IdentityAndAccess;
using Hydra.Services.Cache;
using Hydra.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Hydra.Services
{
    [RegisterAsService(typeof(IService<SystemUserPermission>))]
    public class SystemUserPermissionService : Service<SystemUserPermission>
    {
        private readonly SystemUserService _systemUserService;
        private readonly PermissionService _permissionService;

        public SystemUserPermissionService(ServiceInjector injector,
                                           IQueryableCacheService<Guid, SystemUserPermission> cacheService,
                                           SystemUserService systemUserService,
                                           PermissionService permissionService) : base(injector)
        {
            _systemUserService = systemUserService;
            _permissionService = permissionService;

            SetCacheService(cacheService);
        }

        public async Task<List<SystemUserPermission>> GetSystemUserPermissionsAsync(Expression<Func<SystemUserPermission, bool>> predicate)
        {
            if (CacheService is IQueryableCacheService<Guid, SystemUserPermission> queryableCache &&
                queryableCache.TryGetAll(predicate.Compile(), out var cachedList))
            {
                return cachedList;
            }

            return await SelectThenCache(predicate);
        }

        public async Task<List<SystemUser>> GetUsersAsync(Guid permissionId)
        {
            var systemUserPermissions = await GetSystemUserPermissionsAsync(up => up.PermissionId == permissionId);
            var users = new List<SystemUser>();

            foreach (var up in systemUserPermissions)
            {
                var user = await _systemUserService.GetOrSelectThenCacheAsync(up.SystemUserId);
                if (user != null)
                    users.Add(user);
            }

            return users;
        }

        public async Task<List<Permission>> GetPermissionsAsync(Guid userId)
        {
            var systemUserPermissions = await GetSystemUserPermissionsAsync(up => up.SystemUserId == userId);
            var permissions = new List<Permission>();

            foreach (var up in systemUserPermissions)
            {
                var permission = await _permissionService.GetOrSelectThenCacheAsync(up.PermissionId);
                if (permission != null)
                    permissions.Add(permission);
            }

            return permissions;
        }
    }
}
