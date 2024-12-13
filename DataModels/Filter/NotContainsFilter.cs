using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DataModels.Filter
{
    public class NotContainsFilter : Filter
    {
        public NotContainsFilter(object value) : base(value)
        {

        }

        public NotContainsFilter(string columnName, object value, string? alias = null) : base(columnName, value, alias)
        {

        }

        public NotContainsFilter(IMetaColumn column, object value) : base(column, value)
        {

        }

        public override string PrepareQueryString()
        {
            return $"{Column?.Table?.Alias}.{Column?.Name} not like  \'%\'+@{StartParameterIndex}+\'%\'";
        }
    }
}
