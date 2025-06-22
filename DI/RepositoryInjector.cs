using Hydra.Services;
using Microsoft.EntityFrameworkCore;

namespace Hydra.DI
{
    public class RepositoryInjector : Injector
    {
        public DbContext Context { get; set; }

        public ILogService LogService { get; set; }

        public RepositoryInjector(DbContext context,ILogService logService)
        {
            Context = context;

            LogService = logService;
        }
    }
}
