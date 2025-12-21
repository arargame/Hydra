using Hydra.DAL.Core;
using Hydra.DI;
using Hydra.Core.HumanResources;

namespace Hydra.DAL.Repositories.HumanResources
{
    [RegisterAsRepository(typeof(IRepository<OrganizationUnit>))]
    public class OrganizationUnitRepository : Repository<OrganizationUnit>
    {
        public OrganizationUnitRepository(RepositoryInjector injector)
            : base(injector) { }
    }
}
