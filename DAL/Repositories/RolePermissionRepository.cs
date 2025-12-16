using Hydra.DAL.Core;
using Hydra.DI;
using Hydra.AccessManagement;

namespace Hydra.DAL.Repositories
{
    [RegisterAsRepository(typeof(IRepository<RolePermission>))]
    public class RolePermissionRepository : Repository<RolePermission>
    {
        public RolePermissionRepository(RepositoryInjector injector)
            : base(injector) { }
    }
}
