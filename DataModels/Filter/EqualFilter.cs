using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DataModels.Filter
{
    public class EqualFilter : Filter
    {
        public EqualFilter(object value) : base(value)
        {

        }

        public EqualFilter(string columnName, object value, string? alias = null) : base(columnName, value, alias)
        {

        }

        public EqualFilter(IMetaColumn column, object value) : base(column, value)
        {

        }


        public override string PrepareQueryString()
        {
            return $"{Column?.Table?.Alias}.{Column?.Name}=@{StartParameterIndex}";
        }

    }
}
