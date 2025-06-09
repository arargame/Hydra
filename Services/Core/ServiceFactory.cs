using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Services.Core
{
    public interface IServiceFactory
    {
        object GetService(Type type);
        T GetService<T>() where T : notnull;
    }

    public class ServiceFactory : IServiceFactory
    {
        private readonly IServiceProvider _provider;

        public ServiceFactory(IServiceProvider provider)
        {
            _provider = provider;
        }

        public object GetService(Type type) => _provider.GetRequiredService(type);

        public T GetService<T>() where T : notnull
        {
            return _provider.GetRequiredService<T>();
        }
    }
}
