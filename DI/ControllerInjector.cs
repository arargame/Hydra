﻿using Hydra.CacheManagement.Managers;
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
        public readonly UnitOfWork _unitOfWork;

        public readonly IHttpContextAccessor _httpContextAccessor;

        public readonly IConfiguration Configuration;

        private readonly SessionInformationCacheManager _cacheManager;
        public ControllerInjector(IHttpContextAccessor httpContextAccessor,
                                UnitOfWork unitOfWork,
                                IConfiguration configuration,
                                SessionInformationCacheManager cacheManager)
        {
            _httpContextAccessor = httpContextAccessor;

            _unitOfWork = unitOfWork;

            Configuration = configuration;

            _cacheManager = cacheManager;
        }

        //public static object GetLastSessionLockObject = new object();

        //public SessionInformation? GetSessionInformation()
        //{
        //    var request = HttpContextAccessor.HttpContext.Request;

        //    if (request.Headers.ContainsKey("UserId"))
        //    {
        //        lock (SessionInformationCacheManager)
        //        {
        //            Guid.TryParse(request.Headers["UserId"].FirstOrDefault(), out var userId);

        //            if (SessionInformationCacheManager.TryGetSession(userId, out var sessionInformation))
        //            {
        //                return sessionInformation;
        //            }
        //            else
        //            {
        //                var userService = new SystemUserService(new ServiceInjector(UnitOfWork, null, Configuration));
        //                var session = userService.GetLastSession(userId);

        //                SessionInformationCacheManager.Login(session);
        //                return session;
        //            }
        //        }
        //    }

        //    return null;
        //}
    }
}
