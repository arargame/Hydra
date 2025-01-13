using Hydra.IdentityAndAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.CacheManagement.Managers
{

    //    var sessionTimeout = TimeSpan.FromMinutes(30);
    //    var cleanupInterval = TimeSpan.FromMinutes(5);
    //    var sessionCacheManager = new SessionInformationCacheManager(sessionTimeout, cleanupInterval);

    public class SessionInformationCacheManager
    {
        private readonly TimedCache<Guid, SessionInformation> sessionCache;
        private readonly TimeSpan sessionTimeout;

        public SessionInformationCacheManager(TimeSpan sessionTimeout, TimeSpan cleanupInterval)
        {
            this.sessionTimeout = sessionTimeout;
            sessionCache = new TimedCache<Guid, SessionInformation>(cleanupInterval);
        }

        public void Login(SessionInformation sessionInformation)
        {
            sessionCache.Add(sessionInformation.SystemUserId, sessionInformation, sessionTimeout);
        }

        public bool Logout(Guid userId)
        {
            sessionCache.Remove(userId);
            return true;
        }

        public bool TryGetSession(Guid userId, out SessionInformation sessionInformation)
        {
            return sessionCache.TryGet(userId, out sessionInformation);
        }
    }
}
