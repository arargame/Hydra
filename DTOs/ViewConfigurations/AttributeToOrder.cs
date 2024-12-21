using Hydra.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DTOs.ViewConfigurations
{
    public class AttributeToOrder
    {
        public bool IsOrderable { get; set; }

        public bool IsOrdered
        {
            get
            {
                return SortingDirection != null;
            }
        }
        public SortingDirection? SortingDirection { get; set; }

        public AttributeToOrder() { }

        public AttributeToOrder(bool isOrderable, SortingDirection? sortingDirection = null)
        {
            IsOrderable = isOrderable;

            SortingDirection = sortingDirection;
        }
    }
}
