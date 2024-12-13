using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DataModels.Filter
{
    public class NotEqualFilter : Filter
    {
        public NotEqualFilter(object value) : base(value)
        {

        }

        public NotEqualFilter(string columnName, object value, string? alias = null) : base(columnName, value, alias)
        {

        }

        public NotEqualFilter(IMetaColumn column, object value) : base(column, value)
        {

        }

        public override string PrepareQueryString()
        {
            return $"{Column?.Table?.Alias}.{Column?.Name}!=@{StartParameterIndex}";
        }

    }
}
