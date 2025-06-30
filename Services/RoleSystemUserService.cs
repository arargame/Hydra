using Hydra.DI;
using Hydra.AccessManagement;
using Hydra.Services.Cache;
using Hydra.Services.Core;
using System.Linq.Expressions;

namespace Hydra.Services
{
    [RegisterAsService(typeof(IService<RoleSystemUser>))]
    public class RoleSystemUserService : Service<RoleSystemUser>
    {
        private readonly RoleService _roleService;
        private readonly SystemUserService _systemUserService;

        public RoleSystemUserService(ServiceInjector injector,
                                     IQueryableCacheService<Guid, RoleSystemUser> cache,
                                     RoleService roleService,
                                     SystemUserService systemUserService) : base(injector)
        {
            _roleService = roleService;
            _systemUserService = systemUserService;

            SetCacheService(cache);
        }

        public async Task<List<RoleSystemUser>> GetRoleSystemUsersAsync(Expression<Func<RoleSystemUser, bool>> predicate)
        {
            if (CacheService is IQueryableCacheService<Guid, RoleSystemUser> queryableCache &&
                queryableCache.TryGetAll(predicate.Compile(), out var cachedList))
            {
                return cachedList;
            }

            return await SelectThenCache(predicate);
        }

        public async Task<List<Role>> GetRolesAsync(Guid userId)
        {
            var roleSystemUsers = await GetRoleSystemUsersAsync(ru => ru.UserId == userId);
            var roles = new List<Role>();

            foreach (var ru in roleSystemUsers)
            {
                var role = await _roleService.GetOrSelectThenCacheAsync(ru.RoleId);
                if (role != null)
                    roles.Add(role);
            }

            return roles;
        }

        public async Task<List<SystemUser>> GetUsersAsync(Guid roleId)
        {
            var roleSystemUsers = await GetRoleSystemUsersAsync(ru => ru.RoleId == roleId);
            var users = new List<SystemUser>();

            foreach (var ru in roleSystemUsers)
            {
                var user = await _systemUserService.GetOrSelectThenCacheAsync(ru.UserId);
                if (user != null)
                    users.Add(user);
            }

            return users;
        }
    }
}
