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
        public object GetService(Type type);
        
        /// <summary>
        /// Used for eager loading (e.g., in MainController).
        /// Use this when the service is definitely needed and safe to instantiate (no circular dependencies).
        /// </summary>
        T GetService<T>() where T : notnull;

        /// <summary>
        /// Used for lazy loading (e.g., services calling other services).
        /// Use this to avoid circular dependencies and performance overhead when the service is conditionally needed.
        /// </summary>
        Lazy<T> GetServiceLazy<T>() where T : class;
    }

    public class ServiceFactory : IServiceFactory
    {
        private readonly IServiceProvider _provider;
        private readonly System.Collections.Concurrent.ConcurrentDictionary<Type, object> _lazyCache = new();

        public ServiceFactory(IServiceProvider provider)
        {
            _provider = provider;
        }

        public object GetService(Type type) => _provider.GetRequiredService(type);

        public T GetService<T>() where T : notnull
        {
            return _provider.GetRequiredService<T>();
        }

        public Lazy<T> GetServiceLazy<T>() where T : class
        {
            return (Lazy<T>)_lazyCache.GetOrAdd(
                typeof(T),
                _ => new Lazy<T>(() => _provider.GetRequiredService<T>())
            );
        }
    }
}
