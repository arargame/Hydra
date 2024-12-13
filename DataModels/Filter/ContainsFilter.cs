using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DataModels.Filter
{
    public class ContainsFilter : Filter
    {
        public ContainsFilter(object value) : base(value)
        {

        }

        public ContainsFilter(string columnName, object value, string? alias = null) : base(columnName, value, alias)
        {

        }

        public ContainsFilter(IMetaColumn column, object value) : base(column, value)
        {

        }

        public override string PrepareQueryString()
        {
            return $"{Column?.Table?.Alias}.{Column?.Name} like \'%\'+@{StartParameterIndex}+\'%\'";
        }
    }
}
