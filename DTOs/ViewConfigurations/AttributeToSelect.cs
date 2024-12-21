using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DTOs.ViewConfigurations
{
    public class AttributeToSelect
    {
        public int Priority { get; set; }

        public AttributeToSelect() { }
        public AttributeToSelect(int priority)
        {
            Priority = priority;
        }

        public AttributeToSelect SetPripority(int priority)
        {
            Priority = priority;

            return this;
        }
    }
}
