using Hydra.DataModels.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DTOs
{
    public class FilterParameterDTO
    {
        public int Index { get; set; }

        public object? Value { get; set; } = null;

        public FilterParameterDTO() { }

        public FilterParameterDTO(object? value)
        {
            SetValue(value);
        }

        public static FilterParameterDTO? ConvertToFilterParameterDTO(IFilterParameter? filterParameter)
        {
            if (filterParameter == null)
                return null;

            var filterParameterDTO = new FilterParameterDTO()
            {
                Index = filterParameter.Index,
                Value = filterParameter.Value
            };

            return filterParameterDTO;
        }

        public FilterParameterDTO SetValue(object? value)
        {
            Value = value;

            return this;
        }
    }
}
