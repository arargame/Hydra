using Hydra.DI;
using Hydra.IdentityAndAccess;
using Hydra.Services.Cache;
using Hydra.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Services
{
    [RegisterAsService(typeof(IService<Permission>))]
    public class PermissionService : Service<Permission>
    {
        private readonly ICacheService<Guid, Permission> _cache;
        public PermissionService(ServiceInjector injector, ICacheService<Guid, Permission> cache) : base(injector)
        {
            HasCache = true;

            _cache = cache;
        }
    }
}
