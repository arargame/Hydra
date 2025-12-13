using Hydra.DI.HttpContextDI;
using Hydra.DTOs.ViewConfigurations;
using Hydra.IdentityAndAccess;
using Hydra.Services.Core;
using Microsoft.AspNetCore.Http;

namespace Hydra.DI
{
    public interface IControllerInjector
    {
        IServiceFactory ServiceFactory { get; }
        ISessionContext SessionContext { get; }
        //IConfiguration Configuration { get; }
        //HttpContext HttpContext { get; }
        IHttpContextAccessorAbstraction HttpContextAccessor { get; }

        //ILogService LogService { get; }
    }
    public class ControllerInjector : IControllerInjector
    {
        public IServiceFactory ServiceFactory { get; }
        public ISessionContext SessionContext { get; }
        //public IConfiguration Configuration { get; }
        //  public IHttpContextAccessor HttpContextAccessor { get; }
        public IHttpContextAccessorAbstraction HttpContextAccessor { get; }
        //public ILogService LogService { get; }

      //  public HttpContext HttpContext => HttpContextAccessor.HttpContext!;


        public ControllerInjector(
            IServiceFactory serviceFactory,
            ISessionContext sessionContext,
            //            IConfiguration configuration,
            IHttpContextAccessorAbstraction httpContextAccessor)
         //   ILogService logService)
        {
            
            ServiceFactory = serviceFactory;
            SessionContext = sessionContext;
         //   Configuration = configuration;
            HttpContextAccessor = httpContextAccessor;
            //LogService = logService;
        }
    }
}
