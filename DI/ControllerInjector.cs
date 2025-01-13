using Hydra.CacheManagement.Managers;
using Hydra.DAL;
using Hydra.DTOs.ViewConfigurations;
using Hydra.IdentityAndAccess;
using Hydra.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DI
{
    public class ControllerInjector : Injector
    {
        public readonly UnitOfWork UnitOfWork;

        public readonly IHttpContextAccessor HttpContextAccessor;

        public readonly IConfiguration Configuration;

        private readonly SessionInformationCacheManager SessionInformationCacheManager;
        public ControllerInjector(IHttpContextAccessor httpContextAccessor,
                                UnitOfWork unitOfWork,
                                IConfiguration configuration,
                                SessionInformationCacheManager cacheManager)
        {
            HttpContextAccessor = httpContextAccessor;

            UnitOfWork = unitOfWork;

            Configuration = configuration;

            SessionInformationCacheManager = cacheManager;
        }

        public static object GetLastSessionLockObject = new object();

        public SessionInformation? GetSessionInformation()
        {
            var request = HttpContextAccessor.HttpContext.Request;

            if (request.Headers.ContainsKey("UserId"))
            {
                lock (SessionInformationCacheManager)
                {
                    Guid.TryParse(request.Headers["UserId"].FirstOrDefault(), out var userId);

                    if (SessionInformationCacheManager.TryGetSession(userId, out var sessionInformation))
                    {
                        return sessionInformation;
                    }
                    else
                    {
                        var userService = new SystemUserService(new ServiceInjector(UnitOfWork, null, Configuration));
                        var session = userService.GetLastSession(userId);

                        SessionInformationCacheManager.Login(session);
                        return session;
                    }
                }
            }

            return null;
        }
    }
}
