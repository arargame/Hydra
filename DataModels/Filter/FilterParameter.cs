using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DataModels.Filter
{
    public interface IFilterParameter
    {
        IFilter? Filter { get; set; }
        int Index { get; set; }

        object? Value { get; set; }

        IFilterParameter SetFilter(IFilter filter);

    }
    public class FilterParameter : IFilterParameter
    {
        public IFilter? Filter { get; set; } = null;

        public int Index { get; set; }

        public object? Value { get; set; } = null;

        public FilterParameter(object? value)
        {
            Value = value;
        }

        public FilterParameter(int index, object value)
        {
            Index = index;

            Value = value;
        }

        public IFilterParameter SetFilter(IFilter filter)
        {
            Filter = filter;

            return this;
        }
    }
}
