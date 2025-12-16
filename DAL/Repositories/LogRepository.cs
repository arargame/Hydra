using Hydra.DAL.Core;
using Hydra.DI;
using Hydra.Core;

namespace Hydra.DAL.Repositories
{
    [RegisterAsRepository(typeof(IRepository<Log>))]
    public class LogRepository : Repository<Log>
    {
        public LogRepository(RepositoryInjector injector)
            : base(injector) { }
    }
}
