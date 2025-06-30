using Hydra.AccessManagement;
using Hydra.IdentityAndAccess;
using Hydra.Services.Cache;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.CacheManagement.Managers
{

    public class SessionInformationCacheManager
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _sessionTimeout;

        public SessionInformationCacheManager(IMemoryCache cache, TimeSpan sessionTimeout)
        {
            _cache = cache;
            _sessionTimeout = sessionTimeout;
        }

        public bool Login(SessionInformation session)
        {
            _cache.Set(session.SystemUserId, session, _sessionTimeout);

            return true;
        }

        public bool Logout(Guid userId)
        {
            _cache.Remove(userId);

            return true;
        }

        public bool TryGetSession(Guid userId, out SessionInformation? session)
        {
            return _cache.TryGetValue(userId, out session);
        }
    }


}
