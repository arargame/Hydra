using Hydra.DAL.Core;
using Hydra.DI;
using Hydra.AccessManagement;

namespace Hydra.DAL.Repositories
{
    [RegisterAsRepository(typeof(IRepository<SystemUserPermission>))]
    public class SystemUserPermissionRepository : Repository<SystemUserPermission>
    {
        public SystemUserPermissionRepository(RepositoryInjector injector)
            : base(injector) { }
    }
}
