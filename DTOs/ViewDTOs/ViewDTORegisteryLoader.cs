using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DTOs.ViewDTOs
{
    public static class ViewDTORegistryLoader
    {
        public static void LoadAllViewDTOs(params Assembly[] assemblies)
        {
            var dtoTypes = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && typeof(ViewDTO).IsAssignableFrom(t))
                .Where(t => t.GetCustomAttribute<RegisterAsViewDTO>() != null);

            foreach (var dtoType in dtoTypes)
            {
                var attr = dtoType.GetCustomAttribute<RegisterAsViewDTO>();
                if (attr == null) continue;

                foreach (var viewType in Enum.GetValues(typeof(ViewType)).Cast<ViewType>())
                {
                    ViewDTOConfigurationCacheManager.Instance.GetOrLoad(dtoType, attr.TableName, viewType);
                }
            }
        }
    }

}
