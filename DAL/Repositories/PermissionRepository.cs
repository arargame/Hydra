using Hydra.DAL.Core;
using Hydra.DI;
using Hydra.IdentityAndAccess;

namespace Hydra.DAL.Repositories
{
    [RegisterAsRepository(typeof(IRepository<Permission>))]
    public class PermissionRepository : Repository<Permission>
    {
        public PermissionRepository(RepositoryInjector injector)
            : base(injector) { }
    }
}
