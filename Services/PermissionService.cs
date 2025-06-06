using Hydra.DI;
using Hydra.IdentityAndAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Services
{
    public class PermissionService : Service<Permission>
    {
        public PermissionService(ServiceInjector injector) : base(injector)
        {
            HasCache = true;
        }
    }
}
