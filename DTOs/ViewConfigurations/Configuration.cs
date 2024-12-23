using Hydra.DataModels;
using Hydra.DTOs.ViewDTOs;
using Hydra.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DTOs.ViewConfigurations
{
    public class ConfigurationCacheGroup
    {
        public string? ViewName { get; set; } = null;

        public ViewType ViewType { get; set; }

        public List<IConfiguration> List { get; set; } = new List<IConfiguration>();
    }

    public interface IConfiguration
    {
        string? TableName { get; set; }

        PropertyInfo? PropertyInfo { get; set; }

        string? PropertyName { get; }

        ViewType ViewType { get; set; }

        string? ViewName { get; set; }

        string? DisplayName { get; set; }

        AttributeToSelect? ToSelect { get; set; }

        AttributeToFilter? ToFilter { get; set; }

        AttributeToOrder? ToOrder { get; set; }

        HtmlElementType ElementType { get; set; }

        HtmlInputType InputType { get; set; }

        object? DefaultValue { get; set; }

        bool IsForeignKey { get; set; }

        bool IsNavigation { get; }
        bool IsPrimaryKey { get; }

        bool CreateResultViewFromThis { get; set; }

        IConfiguration SetPropertyInfo(PropertyInfo propertyInfo);

        IConfiguration SetDisplayName(string displayName);

        IConfiguration SetNavigationColumnInfo(NavigationColumnInfo navigation);

        IConfiguration SetTableName(string? tableName);

        MetaColumnDTO ToColumnDTO(string columnTypeName);

        IConfiguration SetPriorityToSelect(int priority);

        IConfiguration SetPriorityToFilter(int priority);

        IConfiguration SetDefaultValue(object defaultValue);

        IConfiguration SetToCreateResultViewFromThis(bool enable);
    }

    public class Configuration : IConfiguration
    {
        public static List<ConfigurationCacheGroup> CacheGroups = new List<ConfigurationCacheGroup>();

        public string? TableName { get; set; } = null;

        public PropertyInfo? PropertyInfo { get; set; }=null;

        public string? PropertyName
        {
            get
            {
                return PropertyInfo?.Name;
            }
        }

        public ViewType ViewType { get; set; }

        public string? ViewName { get; set; } = null;

        public string? DisplayName { get; set; } = null;

        public AttributeToSelect? ToSelect { get; set; } = null;

        public AttributeToFilter? ToFilter { get; set; } = null;

        public AttributeToOrder? ToOrder { get; set; } = null;

        public HtmlElementType ElementType { get; set; }

        public HtmlInputType InputType { get; set; }

        public object? DefaultValue { get; set; } = null;

        public bool IsForeignKey { get; set; }

        public bool CreateResultViewFromThis { get; set; }


        public NavigationColumnInfo? Navigation { get; set; } = null;

        public bool IsNavigation
        {
            get
            {
                return Navigation != null ? true : false;
            }
        }

        private bool? isPrimaryKey { get; set; }
        public bool IsPrimaryKey
        {
            get
            {
                return isPrimaryKey ?? (IsNavigation && Navigation?.NameToDisplay == "Id" ? true : false);
            }
            set
            {
                isPrimaryKey = value;
            }
        }

        public Configuration(
            PropertyInfo? propertyInfo,
            ViewType viewType,
            string? viewName,
            AttributeToSelect? toSelect,
            AttributeToFilter? toFilter,
            AttributeToOrder? toOrder,
            NavigationColumnInfo? navigation = null,
            HtmlElementType elementType = HtmlElementType.Input,
            HtmlInputType inputType = HtmlInputType.text,
            string? displayName = null,
            object? defaultValue = null,
            bool isForeignKey = false,
            bool setToCreateResultViewFromThis = true)
        {
            PropertyInfo = propertyInfo;

            ViewType = viewType;

            ViewName = viewName;

            ToSelect = toSelect;

            ToFilter = toFilter;

            ToOrder = toOrder;

            Navigation = navigation;

            ElementType = elementType;

            InputType = inputType;

            DisplayName = displayName;

            DefaultValue = defaultValue;

            IsForeignKey = isForeignKey;

            CreateResultViewFromThis = setToCreateResultViewFromThis;
        }

        public Configuration(
            ViewType viewType,
            string? viewName,
            AttributeToSelect? toSelect,
            AttributeToFilter? toFilter,
            AttributeToOrder? toOrder,
            NavigationColumnInfo? navigation = null,
            HtmlElementType elementType = HtmlElementType.Input,
            HtmlInputType inputType = HtmlInputType.text,
            string? displayName = null,
            object? defaultValue = null,
            bool isForeignKey = false,
            bool setToCreateResultViewFromThis = true) : this(propertyInfo: null,
                viewType: viewType,
                viewName: viewName,
                toSelect: toSelect,
                toFilter: toFilter,
                toOrder: toOrder,
                navigation: navigation,
                elementType: elementType,
                inputType: inputType,
                displayName: displayName,
                defaultValue: defaultValue,
                isForeignKey: isForeignKey,
                setToCreateResultViewFromThis: setToCreateResultViewFromThis)
        {

        }

        public IConfiguration SetPropertyInfo(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;

            return this;
        }

        public IConfiguration SetDisplayName(string? displayName)
        {
            DisplayName = displayName;

            return this;
        }

        public IConfiguration SetNavigationColumnInfo(NavigationColumnInfo? navigation)
        {
            Navigation = navigation;

            return this;
        }

        public Configuration SetViewType(ViewType viewType)
        {
            ViewType = viewType;

            return this;
        }

        public Configuration SetViewName(string viewName)
        {
            ViewName = viewName;

            return this;
        }

        public Configuration SetAttributeToSelect(AttributeToSelect toSelect)
        {
            ToSelect = toSelect;

            return this;
        }

        public Configuration SetAttributeToFilter(AttributeToFilter toFilter)
        {
            ToFilter = toFilter;

            return this;
        }

        public Configuration SetAttributeToOrder(AttributeToOrder toOrder)
        {
            ToOrder = toOrder;

            return this;
        }

        public Configuration SetElementType(HtmlElementType elementType)
        {
            ElementType = elementType;

            return this;
        }

        public Configuration SetInputType(HtmlInputType inputType)
        {
            InputType = inputType;

            return this;
        }


        public IConfiguration SetDefaultValue(object defaultValue)
        {
            DefaultValue = defaultValue;

            return this;
        }

        public Configuration SetIsForeignKey(bool isForeignKey)
        {
            IsForeignKey = isForeignKey;

            return this;
        }

        public Configuration SetIsPrimaryKey(bool isPrimaryKey)
        {
            IsPrimaryKey = isPrimaryKey;

            return this;
        }

        public IConfiguration SetTableName(string? tableName)
        {
            TableName = tableName;

            return this;
        }

        public IConfiguration SetPriorityToSelect(int priority)
        {
            ToSelect.SetPripority(priority);

            return this;
        }

        public IConfiguration SetPriorityToFilter(int priority)
        {
            ToFilter.SetPripority(priority);

            return this;
        }

        public IConfiguration SetToCreateResultViewFromThis(bool enable)
        {
            CreateResultViewFromThis = enable;

            return this;
        }


        public MetaColumnDTO ToColumnDTO(string columnTypeName)
        {
            var columnDTO = new MetaColumnDTO()
            {
                Name = PropertyName,
                TypeName = columnTypeName,
                DisplayName = DisplayName,
                Priority = ToSelect != null ? ToSelect.Priority : 0,
                IsForeignKey = IsForeignKey,
                HtmlElementType = ElementType,
                HtmlInputType = InputType,
                NavigationColumnInfoDTO = Navigation != null ? NavigationColumnInfoDTO.ConvertToNavigationColumnInfoDTO(Navigation) : null,
                FilterDTO = new FilterDTO(typeName: ToFilter != null ? ToFilter.TypeName : null,
                                            priority: ToFilter != null ? ToFilter.Priority : 0,
                                            values: null,
                                            createFilterComponentFromThis: ToFilter != null ? ToFilter.CreateFilterComponentFromThis : false),
                IsOrderable = ToOrder != null && ToOrder.IsOrderable,
                IsOrdered = ToOrder != null && ToOrder.IsOrdered,
                Direction = (ToOrder != null && ToOrder.SortingDirection != null) ? ToOrder.SortingDirection.Value : SortingDirection.Ascending,
                ValueType = PropertyInfo.GetPrimitiveTypeName(),
                PropertyTypeName = ReflectionHelper.IsPropertyNullable(PropertyInfo) ? ReflectionHelper.GetNullablePropertyName(PropertyInfo) : PropertyInfo.PropertyType.Name,
                DefaultValue = DefaultValue,
                TableName = TableName,
                ViewType = ViewType,
                IsDisabled = ToFilter != null ? ToFilter.IsDisabled : false,
                CreateResultViewFromThis = CreateResultViewFromThis
            };

            if (columnDTO.DefaultValue != null)
            {
                columnDTO.FilterDTO.SetParameters(new List<object?>() { DefaultValue });
            }

            columnDTO.SetAsPrimaryKey(columnDTO.IsNavigation && columnDTO.NavigationColumnInfoDTO?.NameToDisplay == "Id");

            return columnDTO;
        }
    }
}
