using Hydra.DataModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Hydra.DataModels.SortingFilterDirectionExtension;
using System.Collections.Concurrent;

namespace Hydra.Utils
{
    public static class PropertyInfoExtensions
    {
        private static readonly ConcurrentDictionary<PropertyInfo, ColumnValueType> PropertyCache = new();

        public static ColumnValueType GetPrimitiveTypeName(this PropertyInfo pi)
        {
            if (PropertyCache.TryGetValue(pi, out var cachedValue))
            {
                return cachedValue;
            }

            ColumnValueType valueType = ColumnValueType.Object;

            if (ReflectionHelper.IsPropertyFrom<bool>(pi))
            {
                valueType = ColumnValueType.Boolean;
            }
            else if (ReflectionHelper.IsPropertyFrom<Enum>(pi))
            {
                valueType = ColumnValueType.Enum;
            }
            else if (ReflectionHelper.IsPropertyFrom<Guid>(pi))
            {
                valueType = ColumnValueType.Guid;
            }
            else if (ReflectionHelper.IsPropertyFrom<int>(pi))
            {
                valueType = ColumnValueType.Int;
            }
            else if (ReflectionHelper.IsPropertyFrom<double>(pi))
            {
                valueType = ColumnValueType.Double;
            }
            else if (ReflectionHelper.IsPropertyFrom<float>(pi))
            {
                valueType = ColumnValueType.Float;
            }
            else if (ReflectionHelper.IsPropertyFrom<string>(pi))
            {
                valueType = ColumnValueType.String;
            }
            else if (ReflectionHelper.IsPropertyFrom<byte[]>(pi))
            {
                valueType = ColumnValueType.ByteArray;
            }
            else if (ReflectionHelper.IsPropertyFrom<DateTime>(pi))
            {
                valueType = ColumnValueType.DateTime;
            }

            return valueType;
        }


        //public static MetaColumnDTO ToColumnDTOUsingAttributes(this PropertyInfo pi, string columnTypeName)
        //{
        //    var navigation = NavigationAttribute.Get(pi);

        //    var priority = PriorityAttribute.Get(pi);

        //    var displayName = DisplayNameAttribute.Get(pi);

        //    var filterType = FilterTypeAttribute.Get(pi);

        //    var orderableAttribute = columnTypeName == nameof(OrderedColumn) ? OrderableAttribute.Get(pi) : null;

        //    var isOrderedAttribute = columnTypeName == nameof(OrderedColumn) ? IsOrderedAttribute.Get(pi) : null;

        //    var htmlElementType = ElementTypeAttribute.Get(pi);

        //    var htmlInputType = InputTypeAttribute.Get(pi);

        //    var defaultValueAttribute = DefaultValueAttribute.Get(pi);

        //    var columnDTO = new MetaColumnDTO()
        //    {
        //        Name = pi.Name,
        //        TypeName = columnTypeName,
        //        DisplayName = displayName?.Text,
        //        Priority = (priority?.Value) != null ? (int)(priority?.Value) : 0,
        //        IsForeignKey = IsForeignKeyAttribute.Get(pi),
        //        HtmlElementType = htmlElementType != null ? htmlElementType.ElementType : HtmlElementType.Input,
        //        HtmlInputType = htmlInputType != null ? htmlInputType.InputType : HtmlInputType.text,
        //        NavigationColumnInfoDTO = navigation != null ? new NavigationColumnInfoDTO()
        //        {
        //            LeftTableName = navigation?.LeftTableName,
        //            LeftTableKeyName = navigation?.LeftTableKeyName,
        //            RightTableKeyName = navigation?.RightTableKeyName,
        //            RightTableName = navigation?.RightTableName,
        //            NameToDisplay = navigation?.ColumnNameToDisplay,
        //            SetAsLink = (bool)(navigation?.SetAsLink)
        //        } : null,
        //        FilterDTO = new FilterDTO()
        //        {
        //            TypeName = filterType?.TypeName,
        //            Priority = filterType != null ? filterType.Priority : 0
        //        },
        //        IsOrderable = orderableAttribute != null ? orderableAttribute.Enable : false,
        //        IsOrdered = isOrderedAttribute != null,
        //        Direction = isOrderedAttribute != null ? isOrderedAttribute.SortingDirection : SortingDirection.Ascending,
        //        ValueType = pi.GetPrimitiveTypeName(),
        //        PropertyTypeName = pi.PropertyType.Name,
        //        DefaultValue = defaultValueAttribute?.Value
        //    };

        //    columnDTO.SetAsPrimaryKey(columnDTO.IsNavigation && columnDTO.NavigationColumnInfoDTO?.NameToDisplay == "Id");

        //    return columnDTO;
        //}
    }
}
