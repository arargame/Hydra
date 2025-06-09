using Hydra.DAL;
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
    [RegisterAsService(typeof(IService<SystemUser>))]
    public class SystemUserService : Service<SystemUser>
    {
        private readonly ICacheService<Guid, SystemUser> _cache;
        public SystemUserService(ServiceInjector injector, ICacheService<Guid, SystemUser> cache) : base(injector)
        {
            HasCache = true;

            _cache = cache;
        }
    }
}
