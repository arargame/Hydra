using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DataModels.Filter
{
    public class LessThanOrEqualFilter : Filter
    {
        public LessThanOrEqualFilter(object value) : base(value)
        {

        }

        public LessThanOrEqualFilter(string columnName, object value, string? alias = null) : base(columnName, value, alias)
        {

        }

        public LessThanOrEqualFilter(IMetaColumn column, object value) : base(column, value)
        {

        }

        public override string PrepareQueryString()
        {
            return $"{Column?.Table?.Alias}.{Column?.Name}<=@{StartParameterIndex}";
        }
    }
}
