using Hydra.DAL.Core;
using Hydra.DI;
using Hydra.Core.HumanResources;

namespace Hydra.DAL.Repositories.HumanResources
{
    [RegisterAsRepository(typeof(IRepository<Employee>))]
    public class EmployeeRepository : Repository<Employee>
    {
        public EmployeeRepository(RepositoryInjector injector)
            : base(injector) { }
    }
}
