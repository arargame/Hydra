using Hydra.DataModels.Filter;
using Hydra.DataModels;
using Hydra.DTOs.ViewDTOs;
using Hydra.FileOperations;
using Hydra.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Hydra.DataModels.SortingFilterDirectionExtension;

namespace Hydra.DTOs
{
    public partial class MetaColumnDTO
    {
        public Guid Id { get; set; }

        public string? TableName { get; set; } = null;

        public string? TypeName { get; set; } = null;

        public string? Name { get; set; } = null;

        public string? Alias { get; set; } = null;

        public int Priority { get; set; }

        public FilterDTO? FilterDTO { get; set; } = null;

        public bool IsPrimaryKey { get; set; }

        public bool IsForeignKey { get; set; }

        public HtmlElementType HtmlElementType { get; set; }

        public HtmlInputType HtmlInputType { get; set; }

        public ColumnValueType ValueType { get; set; }

        public object? DefaultValue { get; set; } = null;

        public NavigationColumnInfoDTO? NavigationColumnInfoDTO { get; set; } = null;

        public FileInfoDTO? FileInfoDTO { get; set; } = null;

        public bool IsDisabled { get; set; }

        public string? PropertyTypeName { get; set; } = null;
        public SortingDirection Direction { get; set; }

        public bool IsOrderable { get; set; }

        public bool IsOrdered { get; set; }

        public ViewType ViewType { get; set; }

        public bool GroupBy { get; set; }

        public bool CreateViewComponentFromThis
        {
            get
            {
                return FilterDTO != null ? FilterDTO.CreateFilterComponentFromThis : false;
            }
        }

        public bool CreateResultViewFromThis { get; set; }


        public bool BelongsToMainTable
        {
            get
            {
                return IsNavigation && IsForeignKey;
            }
        }

        public bool BelongsToJoins
        {
            get
            {
                return IsNavigation && !IsForeignKey;
            }
        }

        public bool IsJoinedTableKey
        {
            get
            {
                return IsNavigation && IsPrimaryKey;
            }
        }

        public bool IsNavigation
        {
            get
            {
                return NavigationColumnInfoDTO != null;
            }
        }

        public bool IsSelfJoined
        {
            get
            {
                return IsNavigation && NavigationColumnInfoDTO.LeftTableName == NavigationColumnInfoDTO.RightTableName;
            }
        }

        public bool IsFileColumn
        {
            get
            {
                return ValueType == ColumnValueType.ByteArray && (IsNavigation || TableName == nameof(CustomFile));
            }
        }

        public bool BelongsToCustomFile
        {
            get
            {
                return (IsNavigation && NavigationColumnInfoDTO.RightTableName == nameof(CustomFile)) || TableName == nameof(CustomFile);
            }
        }

        private string? displayName { get; set; } = null;
        public string? DisplayName
        {
            get
            {
                return !string.IsNullOrEmpty(displayName) ? displayName : Name;
            }
            set
            {
                displayName = value;
            }
        }

        public bool IsSelectedColumn
        {
            get
            {
                return TypeName == nameof(SelectedColumn);
            }
        }

        public bool IsFilteredColumn
        {
            get
            {
                return TypeName == nameof(FilteredColumn);
            }
        }

        public bool IsOrderedColumn
        {
            get
            {
                return TypeName == nameof(OrderedColumn);
            }
        }

        [JsonIgnore]
        public object GetFirstValue
        {
            get
            {
                return GetParameters?.FirstOrDefault()?.Value;
            }
        }

        [JsonIgnore]
        public List<FilterParameterDTO> GetParameters
        {
            get
            {
                if (FilterDTO != null && FilterDTO.Parameters != null && FilterDTO.Parameters.Any())
                    return FilterDTO.Parameters;

                return null;
            }
        }

        public MetaColumnDTO()
        {
            //Task.Run(() => 
            //{
            //    Id = Guid.NewGuid();
            //});

            Id = Guid.NewGuid();
        }

        public MetaColumnDTO Alter(MetaColumnDTO column)
        {
            if (column.TypeName == nameof(FilteredColumn) && column.FilterDTO == null)
                throw new Exception("The filter of filtered column should not be null");

            FilterDTO = column.FilterDTO;
            Direction = column.Direction;
            Alias = column.Alias;
            Priority = column.Priority;
            DisplayName = column.DisplayName;
            Id = column.Id;
            ViewType = column.ViewType;
            TableName = column.TableName;
            HtmlElementType = column.HtmlElementType;
            HtmlInputType = column.HtmlInputType;

            return this;
        }

        public MetaColumnDTO SetFirstParameterValue(object value)
        {
            var firstParameter = GetParameters?.FirstOrDefault();

            if (firstParameter != null)
                firstParameter.SetValue(value);

            return this;
        }

        public MetaColumnDTO SetViewType(ViewType viewType)
        {
            ViewType = viewType;

            return this;
        }

        public MetaColumnDTO SetDisplayName(string displayName)
        {
            DisplayName = displayName;

            return this;
        }

        public MetaColumnDTO SetAlias(string alias)
        {
            Alias = alias;

            return this;
        }

        public MetaColumnDTO SetAsPrimaryKey(bool enable = true)
        {
            IsPrimaryKey = enable;

            // SetColumnValueType(ColumnValueType.Guid);

            return this;
        }

        public MetaColumnDTO SetAsForeignKey(bool enable = true)
        {
            IsForeignKey = enable;

            return this;
        }

        public MetaColumnDTO SetNavigationColumnInfoDTO(NavigationColumnInfoDTO navigationColumnInfoDTO)
        {
            NavigationColumnInfoDTO = navigationColumnInfoDTO;

            return this;
        }

        public MetaColumnDTO SetColumnValueType(ColumnValueType valueType)
        {
            ValueType = valueType;

            return this;
        }

        public MetaColumnDTO SetDefaultValue(object defaultValue)
        {
            DefaultValue = defaultValue;

            return this;
        }

        public MetaColumnDTO SetTableName(string? tableName)
        {
            TableName = tableName;

            return this;
        }

        public MetaColumnDTO SetHtmlInputType(HtmlInputType htmlInputType)
        {
            HtmlInputType = htmlInputType;

            return this;
        }

        public MetaColumnDTO SetHtmlElementType(HtmlElementType htmlElementType)
        {
            HtmlElementType = htmlElementType;

            return this;
        }

        public MetaColumnDTO SetGroupBy(bool enable = true)
        {
            GroupBy = enable;

            return this;
        }

        public MetaColumnDTO SetDisabled(bool enable = true)
        {
            IsDisabled = enable;

            return this;
        }

        public MetaColumnDTO MakeOrdered(string direction = null, bool isOrderable = true, bool isOrdered = true)
        {
            MakeOrderable(direction, isOrderable);

            IsOrdered = isOrdered;

            return this;
        }

        public MetaColumnDTO MakeOrderable(string direction = null, bool isOrderable = true)
        {
            IsOrderable = isOrderable;

            Direction = direction?.ToLower() switch
            {
                "asc" or "ascending" => SortingDirection.Ascending,
                "desc" or "descending" => SortingDirection.Descending,
                _ => SortingDirection.Ascending,
            };

            return this;
        }


        public string GetControllerNameToNavigate()
        {
            string controllerName = "";

            try
            {
                //controllerName = Helper.GetPropertyOf(type: Helper.GetTypeFromAssembly(typeof(BaseObject), NavigationColumnInfoDTO.LeftTableName),
                //                                propertyName: NavigationColumnInfoDTO.LeftTableKeyName.Replace("Id", "")).PropertyType.Name;
                controllerName = NavigationColumnInfoDTO.RightTableName;
            }
            catch (Exception ex)
            {
                controllerName = TableName;
            }

            return controllerName;
        }


        public static MetaColumnDTO Create(string name, string alias, string typeName, FilterDTO filter, int priority = 0)
        {
            return new MetaColumnDTO()
            {
                Name = name,
                Alias = alias ?? name,
                TypeName = typeName,
                Priority = priority,
                FilterDTO = filter
            };
        }

        public static MetaColumnDTO CreateColumnDTOWithFilter(string name, string alias, int priority, string filterTypeName, int filterPriority, bool createFilterComponentFromThis, List<object> values)
        {
            return Create(name: name,
                            alias: alias,
                            typeName: nameof(FilteredColumn),
                            priority: priority,
                            filter: new FilterDTO(typeName: filterTypeName,
                                                values: values,
                                                priority: filterPriority,
                                                createFilterComponentFromThis: createFilterComponentFromThis));
        }

        public static MetaColumnDTO CreateColumnDTOToSelect(string name, string alias, int priority = 0)
        {
            return Create(name: name,
                            alias: alias,
                            typeName: nameof(SelectedColumn),
                            priority: priority,
                            filter: null);
        }

        public static MetaColumnDTO CreateColumnDTOToOrder(string name, string alias, SortingDirection? sortingDirection = null)
        {
            var column = Create(name: name,
                            alias: alias,
                            typeName: nameof(OrderedColumn),
                            filter: null);

            column.IsOrderable = true;

            column.IsOrdered = sortingDirection != null;

            column.Direction = sortingDirection != null ? sortingDirection.Value : SortingDirection.Ascending;

            return column;
        }

        public static MetaColumnDTO CreateColumnDTOAsForeignKey(string name, string alias, object value, int priority = 0, int filterPriority = 0, bool useFilterToCreateViewComponent = true)
        {
            var column = CreateColumnDTOWithEqualFilter(name: name, alias: alias, value: value, priority: priority, filterPriority: filterPriority, useFilterToCreateViewComponent: useFilterToCreateViewComponent);

            return column.SetAsForeignKey(true).SetColumnValueType(ColumnValueType.Guid);
        }

        public static MetaColumnDTO CreateColumnDTOWithEqualFilter(string name, string alias, object value, int priority = 0, int filterPriority = 0, bool useFilterToCreateViewComponent = true)
        {
            return CreateColumnDTOWithFilter(name: name, alias: alias, priority: priority, filterTypeName: typeof(EqualFilter).Name, createFilterComponentFromThis: useFilterToCreateViewComponent, filterPriority: filterPriority, values: new List<object>() { value });
        }

        public static MetaColumnDTO CreateColumnDTOWithNotEqualFilter(string name, string alias, object value, int priority = 0, int filterPriority = 0, bool useFilterToCreateViewComponent = true)
        {
            return CreateColumnDTOWithFilter(name: name, alias: alias, priority: priority, filterTypeName: typeof(NotEqualFilter).Name, createFilterComponentFromThis: useFilterToCreateViewComponent, filterPriority: filterPriority, values: new List<object>() { value });
        }

        public static MetaColumnDTO CreateColumnDTOWithNullFilter(string name, string alias, int priority = 0, int filterPriority = 0, bool useFilterToCreateViewComponent = true)
        {
            return CreateColumnDTOWithFilter(name: name, alias: alias, priority: priority, filterTypeName: typeof(IsNullFilter).Name, createFilterComponentFromThis: useFilterToCreateViewComponent, filterPriority: filterPriority, values: new List<object>() { null });
        }

        public static MetaColumnDTO CreateColumnDTOWithNotNullFilter(string name, string alias, int priority = 0, int filterPriority = 0, bool useFilterToCreateViewComponent = true)
        {
            return CreateColumnDTOWithFilter(name: name, alias: alias, priority: priority, filterTypeName: typeof(IsNotNullFilter).Name, createFilterComponentFromThis: useFilterToCreateViewComponent, filterPriority: filterPriority, values: new List<object>() { null });
        }

        public static MetaColumnDTO CreateColumnDTOWithContainsFilter(string name, string alias, object value, int priority = 0, int filterPriority = 0, bool useFilterToCreateViewComponent = true)
        {
            return CreateColumnDTOWithFilter(name: name, alias: alias, priority: priority, filterTypeName: typeof(ContainsFilter).Name, createFilterComponentFromThis: useFilterToCreateViewComponent, filterPriority: filterPriority, values: new List<object>() { value });
        }

        public static MetaColumnDTO CreateColumnDTOWithInFilter(string name, string alias, List<object> values, int priority = 0, int filterPriority = 0, bool useFilterToCreateViewComponent = true)
        {
            return CreateColumnDTOWithFilter(name: name, alias: alias, priority: priority, filterTypeName: typeof(InFilter).Name, createFilterComponentFromThis: useFilterToCreateViewComponent, filterPriority: filterPriority, values: values);
        }

        public static MetaColumnDTO ConvertToColumnDTO(IMetaColumn column)
        {
            var columnTypeName = column.GetType().Name;

            var columnDTO = new MetaColumnDTO()
            {
                Id = column.Id,
                TableName = column.Table?.Name,
                TypeName = columnTypeName,
                Name = column.Name,
                Alias = column.Alias,
                FilterDTO = FilterDTO.ConvertToFilterDTO(column.Filter),
                IsForeignKey = column.IsForeignKey,
                HtmlElementType = column.HtmlElementType,
                HtmlInputType = column.HtmlInputType,
                NavigationColumnInfoDTO = NavigationColumnInfoDTO.ConvertToNavigationColumnInfoDTO(column.NavigationColumnInfo),
                IsPrimaryKey = column.IsPrimaryKey,
                DisplayName = column.DisplayName,
                Direction = column.Direction,
                IsOrdered = column.IsOrdered,
                IsOrderable = column.IsOrderable,
                ValueType = column.ValueType,
                PropertyTypeName = column.PropertyTypeName,
                Priority = column.Priority,
                DefaultValue = column.DefaultValue,
                GroupBy = (column is SelectedColumn) ? (column as SelectedColumn)!.GroupBy : false,
                CreateResultViewFromThis = column.CreateResultViewFromThis
            };

            return columnDTO;
        }

        public static IMetaColumn ConvertToColumn(MetaColumnDTO columnDTO)
        {
            var columnTypeName = columnDTO.TypeName;

            IMetaColumn column = null;

            switch (columnTypeName)
            {
                case nameof(FilteredColumn):

                    var filterTypeName = columnDTO.FilterDTO.TypeName;

                    if (columnDTO.FilterDTO.Parameters == null || !columnDTO.FilterDTO.Parameters.Any())
                        break;

                    List<string> values = new List<string>();

                    values = columnDTO.FilterDTO.Parameters.Select(p => p.Value?.ToString()).ToList();

                    switch (filterTypeName)
                    {

                        case nameof(BetweenFilter):

                            column = new FilteredColumn(columnDTO.Name, new BetweenFilter(new FilterParameter(values[0]), new FilterParameter(values[1])));

                            break;


                        case nameof(ContainsFilter):

                            column = new FilteredColumn(columnDTO.Name, new ContainsFilter(values.First()));

                            break;


                        case nameof(EqualFilter):

                            column = new FilteredColumn(columnDTO.Name, new EqualFilter(values.First()));

                            break;

                        case nameof(GreaterThanFilter):

                            column = new FilteredColumn(columnDTO.Name, new GreaterThanFilter(values.First()));

                            break;

                        case nameof(GreaterThanOrEqualFilter):

                            column = new FilteredColumn(columnDTO.Name, new GreaterThanOrEqualFilter(values.First()));

                            break;

                        case nameof(IsNullFilter):

                            column = new FilteredColumn(columnDTO.Name, new IsNullFilter());

                            break;


                        case nameof(IsNotNullFilter):

                            column = new FilteredColumn(columnDTO.Name, new IsNotNullFilter());

                            break;

                        case nameof(InFilter):

                            column = new FilteredColumn(columnDTO.Name, new InFilter(values.Select(v => new FilterParameter(v)).ToArray()));

                            break;

                        case nameof(LessThanFilter):

                            column = new FilteredColumn(columnDTO.Name, new LessThanFilter(values.First()));

                            break;

                        case nameof(LessThanOrEqualFilter):

                            column = new FilteredColumn(columnDTO.Name, new LessThanOrEqualFilter(values.First()));

                            break;

                        case nameof(NotContainsFilter):

                            column = new FilteredColumn(columnDTO.Name, new NotContainsFilter(values.First()));

                            break;

                        case nameof(NotEqualFilter):

                            column = new FilteredColumn(columnDTO.Name, new NotEqualFilter(values.First()));

                            break;

                        case nameof(NotInFilter):

                            column = new FilteredColumn(columnDTO.Name, new NotInFilter(values.Select(v => new FilterParameter(v)).ToArray()));

                            break;


                        default:
                            break;
                    }

                    break;

                case nameof(SelectedColumn):

                    column = new SelectedColumn(columnDTO.Name, columnDTO.Alias, columnDTO.GroupBy);

                    break;

                case nameof(OrderedColumn):

                    column = new OrderedColumn(columnDTO.Name, columnDTO.Direction.ConvertToString());

                    column.IsOrdered = columnDTO.IsOrdered;

                    column.IsOrderable = columnDTO.IsOrderable;

                    break;

                default:
                    break;
            }


            if (column != null)
            {
                column.Id = columnDTO.Id;

                column.NavigationColumnInfo = NavigationColumnInfoDTO.ConvertToNavigationColumnInfo(columnDTO.NavigationColumnInfoDTO);

                column.IsPrimaryKey = columnDTO.IsPrimaryKey;

                column.IsForeignKey = columnDTO.IsForeignKey;

                column.HtmlElementType = columnDTO.HtmlElementType;

                column.HtmlInputType = columnDTO.HtmlInputType;

                column.DisplayName = columnDTO.DisplayName;

                if (column.Filter != null && columnDTO.FilterDTO != null)
                {
                    column.Filter.SetPriority(columnDTO.FilterDTO.Priority);

                    column.Filter.CreateFilterComponentFromThis = columnDTO.FilterDTO.CreateFilterComponentFromThis;
                }

                column.ValueType = columnDTO.ValueType;

                column.PropertyTypeName = columnDTO.PropertyTypeName;

                column.Priority = columnDTO.Priority;

                column.DefaultValue = columnDTO.DefaultValue;

                column.CreateResultViewFromThis = columnDTO.CreateResultViewFromThis;
            }

            return column;
        }

        public object GetValueToDisplayFrom(object result, string propertyName = null)
        {
            if (result == null)
                return null;

            propertyName = string.IsNullOrEmpty(propertyName) ? (BelongsToJoins ? Alias : Name) : propertyName;

            var valueToDisplay = ReflectionHelper.GetValueOf(result.GetType(), propertyName, result);

            return valueToDisplay;
        }
    }
}
