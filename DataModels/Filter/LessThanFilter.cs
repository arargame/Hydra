using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DataModels.Filter
{
    public class LessThanFilter : Filter
    {
        public LessThanFilter(object value) : base(value)
        {

        }

        public LessThanFilter(string columnName, object value, string? alias = null) : base(columnName, value, alias)
        {

        }

        public LessThanFilter(IMetaColumn column, object value) : base(column, value)
        {

        }

        public override string PrepareQueryString()
        {
            return $"{Column?.Table?.Alias}.{Column?.Name}<@{StartParameterIndex}";
        }
    }
}
