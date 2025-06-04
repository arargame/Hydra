using Hydra.Core;
using Hydra.DI;
using Hydra.IdentityAndAccess;
using Hydra.Services;
using Hydra.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DAL
{
    public interface IRepositoryFactoryService
    {
        IRepository<T>? CreateRepository<T>(DbContext context,SessionInformation sessionInfo) where T : BaseObject<T>;
    }

    public class RepositoryFactoryService : IRepositoryFactoryService
    {
        private readonly LogService LogService;

        public RepositoryFactoryService(LogService logService)
        {
            LogService = logService;
        }

        public IRepository<T>? CreateRepository<T>(DbContext context,SessionInformation sessionInfo) where T : BaseObject<T>
        {
            var injector = new RepositoryInjector(context, LogService);

            var repositoryType = ReflectionHelper.GetTypeFromAssembly(typeof(IRepository<>), string.Format("{0}Repository", typeof(T).Name)) ?? typeof(Repository<T>);

            var instance = ReflectionHelper.CreateInstance(typeName: nameof(repositoryType), parameters: new object[]
                {
                    new object[]
                    {
                        injector
                    }
                });

            return instance as IRepository<T>;
        }
    }


}
