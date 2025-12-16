using Hydra.DAL.Core;
using Hydra.DI;
using Hydra.AccessManagement;

namespace Hydra.DAL.Repositories
{
    [RegisterAsRepository(typeof(IRepository<RoleSystemUser>))]
    public class RoleSystemUserRepository : Repository<RoleSystemUser>
    {
        public RoleSystemUserRepository(RepositoryInjector injector)
            : base(injector) { }
    }
}
