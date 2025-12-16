using Hydra.DI;
using Hydra.AccessManagement;
using Hydra.Services.Cache;
using Hydra.Services.Core;
using System.Linq.Expressions;
using Hydra.IdentityAndAccess;

namespace Hydra.Services
{
    [RegisterAsService(typeof(IService<RolePermission>))]
    public class RolePermissionService : Service<RolePermission>
    {
        private readonly ServiceInjector _injector;

        public RolePermissionService(ServiceInjector injector,
                                      IQueryableCacheService<Guid, RolePermission> cacheService) : base(injector)
        {
            _injector = injector;
            SetCacheService(cacheService); 
        }

        public async Task<List<RolePermission>> GetRolePermissions(Expression<Func<RolePermission, bool>> predicate)
        {
            if (CacheService is IQueryableCacheService<Guid, RolePermission> queryableCache &&
                queryableCache.TryGetAll(predicate.Compile(), out var cachedList))
            {
                return cachedList;
            }

            return await SelectThenCache(predicate);
        }

        public async Task<List<Role>> GetRolesAsync(Guid permissionId)
        {
            var rolePermissions = await GetRolePermissions(rp => rp.PermissionId == permissionId);
            var roles = new List<Role>();

            foreach (var rp in rolePermissions)
            {
                var roleService = _injector.ResolveLazy<RoleService>().Value;
                var role = await roleService.GetOrSelectThenCacheAsync(rp.RoleId);
                if (role != null)
                    roles.Add(role);
            }

            return roles;
        }



        public async Task<List<Permission>> GetPermissionsAsync(Guid roleId)
        {
            var rolePermissions = await GetRolePermissions(rp => rp.RoleId == roleId);
            var permissions = new List<Permission>();

            foreach (var rp in rolePermissions)
            {
                var permissionService = _injector.ResolveLazy<PermissionService>().Value;
                var permission = await permissionService.GetOrSelectThenCacheAsync(rp.PermissionId);
                if (permission != null)
                    permissions.Add(permission);
            }

            return permissions;
        }

    }
}
