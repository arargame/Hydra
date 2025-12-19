using Hydra.Services;
using Microsoft.EntityFrameworkCore;

namespace Hydra.DI
{
    public class RepositoryInjector : Injector
    {
        public DbContext Context { get; set; }

        public RepositoryInjector(DbContext context)
        {
            Context = context;
        }
    }
}
