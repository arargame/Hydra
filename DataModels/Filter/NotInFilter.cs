using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DataModels.Filter
{
    public class NotInFilter : InFilter
    {
        public NotInFilter(params FilterParameter[] filterParameters)
        {
            Initialize();

            AddFilterParameter(filterParameters);

            SetStartParameterIndex(StartParameterIndex);
        }

        public NotInFilter(IMetaColumn column, params FilterParameter[] filterParameters) : this(filterParameters)
        {
            SetColumn(column);
        }

        public override string PrepareQueryString()
        {
            return $"{Column?.Table?.Alias}.{Column?.Name} not in ({string.Join(",", Parameters.Select(p => $"\'\'+@{p.Index}+\'\'"))})";
        }
    }
}
