using Hydra.IdentityAndAccess;
using Hydra.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DI
{
    public class RepositoryInjector : Injector
    {
        public DbContext Context { get; set; }

        public LogService LogService { get; set; }

        public RepositoryInjector(DbContext context,LogService logService)
        {
            Context = context;

            LogService = logService;
        }
    }
}
