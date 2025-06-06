using Hydra.DAL;
using Hydra.DI;
using Hydra.IdentityAndAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Services
{
    public class SystemUserService : Service<SystemUser>
    {
        public SystemUserService(ServiceInjector injector) : base(injector)
        {
            
        }

        public SessionInformation GetLastSession(Guid userId)
        {
            return new SessionInformation
            {
                SystemUserId = userId,
                Ip = "127.0.0.1",
                UserAgent = "Mozilla/5.0",
                IsAuthenticated = true
            };
        }
    }
}
