using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DataModels.Filter
{
    public class IsNullFilter : Filter
    {
        public IsNullFilter() : base(null)
        {

        }

        public IsNullFilter(string columnName, string? alias = null) : base(columnName, null, alias)
        {

        }

        public IsNullFilter(IMetaColumn column) : base(column, null)
        {

        }

        public override string PrepareQueryString()
        {
            return $"{Column?.Table?.Alias}.{Column?.Name} is null";
        }
    }
}
