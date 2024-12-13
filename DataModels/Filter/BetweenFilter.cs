using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DataModels.Filter
{
    public class BetweenFilter : Filter
    {
        public IFilterParameter StartFilterParameter { get; set; }

        public IFilterParameter FinishFilterParameter { get; set; }

        public BetweenFilter(FilterParameter startFilterParameter, FilterParameter finishFilterParameter)
        {
            Initialize();

            AddFilterParameter(startFilterParameter, finishFilterParameter);
            
            SetParameterIndex(StartParameterIndex);

            StartFilterParameter = startFilterParameter;

            FinishFilterParameter = finishFilterParameter;
        }

        public BetweenFilter(IMetaColumn column, FilterParameter startFilterParameter, FilterParameter finishFilterParameter) : this(startFilterParameter, finishFilterParameter)
        {
            SetColumn(column);
        }

        public override string PrepareQueryString()
        {
            StartFilterParameter = Parameters[0];

            FinishFilterParameter = Parameters[1];

             return $"({Column?.Table?.Alias}.{Column?.Name} between @{StartFilterParameter.Index} and @{FinishFilterParameter.Index})";
        }

    }
}
