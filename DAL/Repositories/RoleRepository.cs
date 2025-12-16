using Hydra.DAL.Core;
using Hydra.DI;
using Hydra.AccessManagement;

namespace Hydra.DAL.Repositories
{
    [RegisterAsRepository(typeof(IRepository<Role>))]
    public class RoleRepository : Repository<Role>
    {
        public RoleRepository(RepositoryInjector injector)
            : base(injector) { }
    }
}
