using Hydra.Core;
using Hydra.DI;
using Hydra.AccessManagement;
using Hydra.Services;
using Hydra.Utils;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Hydra.DAL.Core
{
    public interface IRepositoryFactoryService
    {
        IRepository<T>? CreateRepository<T>(DbContext context,params object[] additionalParameters) where T : BaseObject<T>;
    }

    public class RepositoryFactoryService : IRepositoryFactoryService
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly LogService _logService;

        private readonly Assembly[] _assembliesToScan;

        public RepositoryFactoryService(IServiceProvider serviceProvider, LogService logService, Assembly[] assembliesToScan)
        {
            _serviceProvider = serviceProvider;

            _logService = logService;

            _assembliesToScan = assembliesToScan;
        }

        //public IRepository<T>? CreateRepository<T>(DbContext context,SessionInformation sessionInfo) where T : BaseObject<T>
        //{
        //    var injector = new RepositoryInjector(context, _logService);

        //    var repositoryType = ReflectionHelper.GetTypeFromAssembly(typeof(IRepository<>), string.Format("{0}Repository", typeof(T).Name)) ?? typeof(Repository<T>);

        //    var instance = ReflectionHelper.CreateInstance(typeName: nameof(repositoryType), parameters: new object[]
        //        {
        //            new object[]
        //            {
        //                injector
        //            }
        //        });

        //    return instance as IRepository<T>;
        //}

        public IRepository<T>? CreateRepository<T>(DbContext context,
                                                     params object[] additionalParameters) where T : BaseObject<T>
        {
            //var repositoryType = ReflectionHelper.GetTypeFromAssembly(
            //    _assemblyToScan,
            //    $"{typeof(T).Name}Repository"
            //) ?? typeof(Repository<T>);
            var expectedInterfaceType = typeof(IRepository<T>);

            var customRepoType = ReflectionHelper.GetTypeWithAttributeImplementingInterface<RegisterAsRepositoryAttribute>(
                _assembliesToScan,
                expectedInterfaceType
            );

            var repositoryType = customRepoType ?? typeof(Repository<T>);

            var injector = new RepositoryInjector(context, _logService);

            var finalParameters = new object[] { injector }.Concat(additionalParameters).ToArray();

            var instance = ReflectionHelper.CreateInstance(
                _serviceProvider,
                repositoryType,
                finalParameters 
            );

            return instance as IRepository<T>;
        }
    }


}
