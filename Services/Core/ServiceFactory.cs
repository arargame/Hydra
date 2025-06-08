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
        T GetService<T>();
    }

    public class ServiceFactory : IServiceFactory
    {
        private readonly IServiceProvider _provider;

        public ServiceFactory(IServiceProvider provider)
        {
            _provider = provider;
        }

        public T GetService<T>()
        {
            return _provider.GetRequiredService<T>();
        }
    }
}
