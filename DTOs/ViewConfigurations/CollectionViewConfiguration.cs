using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DTOs.ViewConfigurations
{
    public class CollectionViewConfiguration : ListViewConfiguration
    {
        public static IConfiguration Get(ListViewConfiguration listViewConfiguration)
        {
            return listViewConfiguration.ForCollectionView();
        }
    }
}
