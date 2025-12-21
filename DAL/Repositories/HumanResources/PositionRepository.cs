using Hydra.DAL.Core;
using Hydra.DI;
using Hydra.Core.HumanResources;

namespace Hydra.DAL.Repositories.HumanResources
{
    [RegisterAsRepository(typeof(IRepository<Position>))]
    public class PositionRepository : Repository<Position>
    {
        public PositionRepository(RepositoryInjector injector)
            : base(injector) { }
    }
}
