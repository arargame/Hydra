using Hydra.DTOs.ViewConfigurations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DTOs.ViewDTOs
{
    public class ViewDTOConfigurationCacheManager
    {
        private static readonly Lazy<ViewDTOConfigurationCacheManager> _instance =
            new(() => new ViewDTOConfigurationCacheManager());

        public static ViewDTOConfigurationCacheManager Instance => _instance.Value;

        private readonly ConcurrentDictionary<(string dtoName, string tableName, ViewType viewType), ConfigurationCacheGroup> _cache =
            new();

        public ConfigurationCacheGroup GetOrLoad(Type dtoType, string tableName, ViewType viewType)
        {
            var key = (dtoType.Name, tableName, viewType);

            if (_cache.TryGetValue(key, out var existing))
                return existing;

            var dtoInstance = Activator.CreateInstance(dtoType) as ViewDTO;
            dtoInstance?.LoadConfigurations();

            var configurations = dtoInstance?.Configurations
                .Where(c => c.ViewType == viewType)
                .ToList() ?? new();

            var group = new ConfigurationCacheGroup
            {
                ViewName = dtoType.Name,
                TableName = tableName,
                ViewType = viewType,
                List = configurations
            };

            _cache.TryAdd(key, group);
            return group;
        }

        public bool TryGet(Type dtoType, string tableName, ViewType viewType, out ConfigurationCacheGroup? group)
        {
            return _cache.TryGetValue((dtoType.Name, tableName, viewType), out group);
        }
    }

}
