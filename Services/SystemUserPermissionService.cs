using Hydra.DI;
using Hydra.AccessManagement;
using Hydra.Services.Cache;
using Hydra.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hydra.IdentityAndAccess;

namespace Hydra.Services
{
    [RegisterAsService(typeof(IService<SystemUserPermission>))]
    public class SystemUserPermissionService : Service<SystemUserPermission>
    {
        private readonly ServiceInjector _injector;

        public SystemUserPermissionService(ServiceInjector injector,
                                           IQueryableCacheService<Guid, SystemUserPermission> cacheService) : base(injector)
        {
            _injector = injector;
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
                var systemUserService = _injector.GetServiceLazy<SystemUserService>().Value;
                var user = await systemUserService.GetOrSelectThenCacheAsync(up.SystemUserId);
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
                var permissionService = _injector.GetServiceLazy<PermissionService>().Value;
                var permission = await permissionService.GetOrSelectThenCacheAsync(up.PermissionId);
                if (permission != null)
                    permissions.Add(permission);
            }

            return permissions;
        }
    }
}
