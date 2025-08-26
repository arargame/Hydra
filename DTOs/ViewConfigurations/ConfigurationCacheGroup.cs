using Hydra.DTOs.ViewDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DTOs.ViewConfigurations
{
    public class ConfigurationCacheGroup
    {
        public string? ViewName { get; set; } = null;

        public string? TableName { get; set; } = null;

        public ViewType ViewType { get; set; }

        public List<IConfiguration> List { get; set; } = new List<IConfiguration>();
    }
}
