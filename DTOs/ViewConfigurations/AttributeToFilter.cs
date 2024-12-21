using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DTOs.ViewConfigurations
{
    public class AttributeToFilter
    {
        public string TypeName { get; set; }

        public int Priority { get; set; }

        public bool IsDisabled { get; set; }

        public bool CreateFilterComponentFromThis { get; set; }

        public AttributeToFilter() { }
        public AttributeToFilter(string typeName, int priority = 0, bool isDisabled = false, bool createFilterComponentFromThis = true)
        {
            SetTypeName(typeName)
                .SetPripority(priority)
                .SetIsDisabled(isDisabled)
                .SetToCreateFilterComponentFromThis(createFilterComponentFromThis);
        }

        public AttributeToFilter SetTypeName(string typeName)
        {
            TypeName = typeName;

            return this;
        }

        public AttributeToFilter SetPripority(int priority)
        {
            Priority = priority;

            return this;
        }

        public AttributeToFilter SetIsDisabled(bool isDisabled)
        {
            IsDisabled = isDisabled;

            return this;
        }

        public AttributeToFilter SetToCreateFilterComponentFromThis(bool enable)
        {
            CreateFilterComponentFromThis = enable;

            return this;
        }
    }
}
