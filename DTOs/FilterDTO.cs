using Hydra.DataModels.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DTOs
{
    public class FilterDTO
    {
        public string? TypeName { get; set; } = null;

        public string? Name { get; set; } = null;

        public int Priority { get; set; }

        public bool CreateFilterComponentFromThis { get; set; } = true;

        public List<FilterParameterDTO?> Parameters { get; set; } = new();

        public FilterDTO()
        {

        }

        public FilterDTO(string typeName, List<object?> values, int priority, bool createFilterComponentFromThis)
        {
            TypeName = typeName;

            if (values != null)
                SetParameters(values);

            Priority = priority;

            CreateFilterComponentFromThis = createFilterComponentFromThis;
        }

        public FilterDTO SetParameters(List<object?> values)
        {
            Parameters = values.Select(v => new FilterParameterDTO(v)).ToList<FilterParameterDTO?>();

            return this;
        }

        public FilterDTO SetParameters(List<FilterParameterDTO?> filterParameters)
        {
            Parameters = filterParameters;

            return this;
        }

        public FilterDTO SetPriority(int priority)
        {
            Priority = priority;

            return this;
        }

        public FilterDTO ClearParameters()
        {
            if (Parameters != null)
                Parameters.Clear();

            return this;
        }

        //public FilterDTO CreateViewComponentFromThis(bool enable = true)
        //{
        //    UseToCreateViewComponent = enable;

        //    return this;
        //}

        public static FilterDTO Create(string? typeName, List<FilterParameterDTO?> parameters)
        {
            return new FilterDTO()
            {
                TypeName = typeName,
                Parameters = parameters
            };
        }

        public static FilterDTO Create(string? typeName, params object[] values)
        {
            return Create(typeName, values.Select(v => new FilterParameterDTO(v))
                                            .ToList<FilterParameterDTO?>());
        }

        public static FilterDTO? ConvertToFilterDTO(IFilter? filter)
        {
            if (filter == null)
                return null;

            var filterDTO = new FilterDTO()
            {
                TypeName = filter.GetType().Name,
                Name = "",
                Parameters = filter.Parameters.Select(p => FilterParameterDTO.ConvertToFilterParameterDTO(p)).ToList(),
                Priority = filter.Priority,
                CreateFilterComponentFromThis = filter.CreateFilterComponentFromThis
            };

            return filterDTO;
        }
    }
}
