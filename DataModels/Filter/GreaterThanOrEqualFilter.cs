using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DataModels.Filter
{
    public class GreaterThanOrEqualFilter : Filter
    {
        public GreaterThanOrEqualFilter(object value) : base(value)
        {

        }


        public GreaterThanOrEqualFilter(string columnName, object value, string? alias = null) : base(columnName, value, alias)
        {

        }

        public GreaterThanOrEqualFilter(IMetaColumn column, object value) : base(column, value)
        {

        }

        public override string PrepareQueryString()
        {
            return $"{Column?.Table?.Alias}.{Column?.Name}>=@{StartParameterIndex}";
        }
    }
}
