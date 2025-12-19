using Hydra.AccessManagement;
using Hydra.DAL.Core;
using Hydra.DTOs.ViewConfigurations;
using Hydra.Services;
using Hydra.Services.Core;
using Hydra.IdentityAndAccess;
using Hydra.ValidationManagement.Hydra.ValidationManagement;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DI
{
    public class ServiceInjector : Injector
    {
        public IUnitOfWork UnitOfWork { get; set; }

        public IRepositoryFactoryService RepositoryFactory {  get; set; }

        //public SessionInformation SessionInformation { get; set; } 

        public Microsoft.Extensions.Configuration.IConfiguration Configuration { get; set; }

        public IServiceProvider ServiceProvider { get; set; }

        public ITableService TableService {  get; set; }

        public IServiceFactory ServiceFactory { get; set; }
        public ISessionContext SessionContext { get; set; }

        public ServiceInjector(IUnitOfWork unitOfWork,
                            IRepositoryFactoryService repositoryFactory,
                            //SessionInformation sessionInformation,
                            Microsoft.Extensions.Configuration.IConfiguration configuration,
                            IServiceProvider serviceProvider,
                            ITableService tableService,
                            IServiceFactory serviceFactory,
                            ISessionContext sessionContext)
        {
            UnitOfWork = unitOfWork;

            RepositoryFactory = repositoryFactory; 

            //SessionInformation = sessionInformation;

            Configuration = configuration;

            ServiceProvider = serviceProvider;

            TableService = tableService;
            ServiceFactory = serviceFactory;
            SessionContext = sessionContext;
        }

        public Lazy<T> GetServiceLazy<T>() where T : class
        {
            return ServiceFactory.GetServiceLazy<T>();
        }

        public Func<T> ResolveFactory<T>() where T : class
        {
            return () => ServiceProvider.GetRequiredService<T>();
        }
    }
}
