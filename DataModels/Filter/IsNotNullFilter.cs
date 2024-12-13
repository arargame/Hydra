using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DataModels.Filter
{
    public class IsNotNullFilter : Filter
    {
        public IsNotNullFilter() : base(value: null)
        {

        }

        public IsNotNullFilter(string columnName, string? alias = null) : base(columnName, null, alias)
        {

        }

        public IsNotNullFilter(IMetaColumn column) : base(column, null)
        {

        }

        public override string PrepareQueryString()
        {
            return $"{Column?.Table?.Alias} . {Column?.Name} is not null";
        }
    }
}
