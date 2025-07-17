using Hydra.AccessManagement;
using Hydra.DAL.Core;
using Hydra.DTOs.ViewConfigurations;
using Hydra.Services;
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

        public IConfiguration Configuration { get; set; }

        public IServiceProvider ServiceProvider { get; set; }

        public ITableService TableService {  get; set; }

        public ServiceInjector(IUnitOfWork unitOfWork,
                            IRepositoryFactoryService repositoryFactory,
                            //SessionInformation sessionInformation,
                            IConfiguration configuration,
                            IServiceProvider serviceProvider,
                            ITableService tableService)
        {
            UnitOfWork = unitOfWork;

            RepositoryFactory = repositoryFactory; 

            //SessionInformation = sessionInformation;

            Configuration = configuration;

            ServiceProvider = serviceProvider;

            TableService = tableService;
        }

        /// <summary>
        /// Lazily resolves a service instance from the IServiceProvider.
        /// This delays the creation of the service until it's actually needed.
        /// </summary>
        /// <typeparam name="T">The type of the service to resolve.</typeparam>
        /// <returns>A Lazy wrapper around the requested service.</returns>
        /// <exception cref="InvalidOperationException">Thrown if ServiceProvider is not initialized.</exception>
        /// <example>
        /// Usage:
        /// <code>
        /// var lazyService = injector.ResolveLazy&lt;QuestionService&gt;();
        /// // Service is created only when .Value is accessed:
        /// var questions = lazyService.Value.GetQuestions(poolId);
        /// </code>
        /// </example>
        public Lazy<T> ResolveLazy<T>() where T : class
        {
            Ensure.IsTrue(ServiceProvider == null, "ServiceProvider is not initialized.");

            return new Lazy<T>(() => ServiceProvider!.GetRequiredService<T>());
        }

        //private readonly Func<QuestionService> _questionServiceFactory;
        //var service = _questionServiceFactory();
        public Func<T> ResolveFactory<T>() where T : class
        {
            return () => ServiceProvider.GetRequiredService<T>();
        }
    }
}
