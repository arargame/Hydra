using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DTOs.ViewDTOs
{
    public class DetailsViewDTO : ViewDTO, ICollectionHandlerDTO
    {
        public List<CollectionViewDTO> Collections { get; set; } = new();

        public virtual DTO LoadCollections()
        {
            Collections.ForEach(c => c.SetLeftTableName(ControllerName));

            foreach (var item in Collections.Where(c => !c.IsCrossTable))
            {
                item.SetLeftTableName(ControllerName);
                item.SetLeftTableKey("Id");
                item.SetLeftTableKeyValue(Id.ToString());
            }

            return this;
        }
    }
}
