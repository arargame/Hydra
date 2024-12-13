using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DataModels.Filter
{
    public class GreaterThanFilter : Filter
    {
        public GreaterThanFilter(object value) : base(value)
        {

        }

        public GreaterThanFilter(string columnName, object value, string? alias = null) : base(columnName, value, alias)
        {

        }

        public GreaterThanFilter(IMetaColumn column, object value) : base(column, value)
        {

        }

        public override string PrepareQueryString()
        {
            return $"{Column?.Table?.Alias}.{Column?.Name}>@{StartParameterIndex}";
        }
    }
}
