using Hydra.DataModels.Filter;
using Hydra.FileOperations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Hydra.DataModels.SortingFilterDirectionExtension;

namespace Hydra.DataModels
{
    public interface IMetaColumn : IColumn
    {
        IFilter? Filter { get; set; }

        bool IsOrderable { get; set; }

        bool IsOrdered { get; set; }

        bool IsSelectable { get; set; }

        bool IsSelected { get; set; }

        bool IsFilterable { get; set; }
        bool IsFiltered { get; set; }

        SortingDirection Direction { get; set; }

        string TypeName { get; }

        int Priority { get; set; }

        string? DisplayName { get; set; }

        string? PropertyTypeName { get; set; }

        ColumnValueType ValueType { get; set; }

        object? DefaultValue { get; set; }

        bool IsPrimaryKey { get; set; }
        bool IsForeignKey { get; set; }

        bool IsFileColumn { get; }

        HtmlElementType HtmlElementType { get; set; }

        HtmlInputType HtmlInputType { get; set; }

        NavigationColumnInfo? NavigationColumnInfo { get; set; }

        bool IsNavigation { get; }

        bool BelongsToJoins { get; }

        IMetaColumn MakeFilterable();

        IMetaColumn MakeFiltered(IFilter filter);

        IMetaColumn MakeSelectable();

        IMetaColumn MakeSelected();

        IMetaColumn MakeOrdered(string? direction = null, bool isOrderable = true, bool isOrdered = true);

        IMetaColumn MakeOrderable(string? direction = null, bool isOrderable = true);

        IMetaColumn Alter(IMetaColumn column);
    }

    public class MetaColumn : Column, IMetaColumn
    {
        public IFilter? Filter { get; set; } = null;
        public bool IsOrderable { get; set; }

        public bool IsOrdered { get; set; }

        public bool IsSelectable { get; set; }

        public bool IsSelected { get; set; }

        public bool IsFilterable { get; set; }
        public bool IsFiltered { get; set; }

        public int Priority { get; set; }

        public string? DisplayName { get; set; } = null;

        public string? PropertyTypeName { get; set; } = null;
        public SortingDirection Direction { get; set; }

        public ColumnValueType ValueType { get; set; }

        public object? DefaultValue { get; set; } = null;

        public bool IsPrimaryKey { get; set; }
        public bool IsForeignKey { get; set; }

        public HtmlElementType HtmlElementType { get; set; }

        public HtmlInputType HtmlInputType { get; set; }

        public NavigationColumnInfo? NavigationColumnInfo { get; set; } = null;

        public bool IsNavigation
        {
            get
            {
                return NavigationColumnInfo != null;
            }
        }

        public bool BelongsToJoins
        {
            get
            {
                return IsNavigation && !IsForeignKey;
            }
        }


        public bool IsFileColumn
        {
            get
            {
                return ValueType == ColumnValueType.ByteArray && (IsNavigation || Table?.Name == nameof(CustomFile));
            }
        }

        public virtual string TypeName
        {
            get
            {
                return GetType().Name;
            }
        }

        public MetaColumn(string? name, object? value = null, string? alias = null)
        {
            SetName(name);

            SetValue(value);

            SetAlias(string.IsNullOrEmpty(alias) ? Name : alias);
        }

        public IMetaColumn MakeFilterable()
        {
            IsFilterable = true;

            return this;
        }

        public IMetaColumn MakeFiltered(IFilter filter)
        {
            MakeFilterable();

            IsFiltered = true;

            filter.SetColumn(this);

            Filter = filter;

            return this;
        }

        public IMetaColumn MakeSelectable()
        {
            IsSelectable = true;

            return this;
        }

        public IMetaColumn MakeSelected()
        {
            MakeSelectable();

            IsSelected = true;

            return this;
        }

        public IMetaColumn MakeOrdered(string? direction = null, bool isOrderable = true, bool isOrdered = true)
        {
            MakeOrderable(direction, isOrderable);

            IsOrdered = isOrdered;

            return this;
        }

        public IMetaColumn MakeOrderable(string? direction = null, bool isOrderable = true)
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

        public IMetaColumn Alter(IMetaColumn column)
        {
            if (column.TypeName == nameof(FilteredColumn) && column.Filter == null)
                throw new Exception($"The filter of filtered column should not be null(Column Type : {column.TypeName} , Column Name : {column.Name})");

            Id = column.Id;
            Filter = column.Filter;
            Direction = column.Direction;
            Alias = column.Alias;
            Priority = column.Priority;
            DisplayName = column.DisplayName;

            return this;
        }
    }

    public class SelectedColumn : MetaColumn
    {
        public bool GroupBy { get; set; }

        public SelectedColumn(string? name, string? alias = null, bool groupBy = false) : base(name, null, alias)
        {
            GroupBy = groupBy;

            MakeSelected();
        }


        public override string TypeName
        {
            get
            {
                return nameof(SelectedColumn);
            }
        }
    }

    public enum SortingDirection
    {
        Ascending,
        Descending
    }

    public static class SortingFilterDirectionExtension
    {
        public static string ConvertToString(this SortingDirection sortingFilterDirection)
        {
            return sortingFilterDirection switch
            {
                SortingDirection.Ascending => "asc",
                SortingDirection.Descending => "desc",
                _ => "asc",
            };
        }

        public static SortingDirection ConvertToSortingDirection(this string sortingDirectionAsString)
        {
            return sortingDirectionAsString switch
            {
                "asc" => SortingDirection.Ascending,
                "desc" => SortingDirection.Descending,
                _ => SortingDirection.Ascending,
            };
        }

        public class FilteredColumn : MetaColumn
        {
            public FilteredColumn(string name, IFilter filter) : base(name, filter.GetValue, null)
            {
                MakeFiltered(filter);
            }

            public override string TypeName
            {
                get
                {
                    return nameof(FilteredColumn);
                }
            }
        }

        public class OrderedColumn : MetaColumn
        {
            public OrderedColumn(string? name, string direction = "asc") : base(name)
            {
                MakeOrdered(direction);
            }

            public override string TypeName
            {
                get
                {
                    return nameof(OrderedColumn);
                }
            }
        }

        public class AggregatedColumn : MetaColumn
        {
            public string AggregationFunction { get; set; }

            public AggregatedColumn(string? name,string aggregationFunction) : base(name:name)
            {
                AggregationFunction = aggregationFunction;
            }

            public override string TypeName
            {
                get
                {
                    return nameof(AggregatedColumn);
                }
            }

            public string GetAggregationSql()
            {
                return $"{AggregationFunction}({Alias})";
            }
        }

        public class CalculatedColumn : MetaColumn
        {
            public string Expression { get; set; }

            public CalculatedColumn(string? name,string expression) : base(name:name)
            {
                Expression = expression;
            }

            public override string TypeName
            {
                get
                {
                    return nameof(CalculatedColumn);
                }
            }

            public string GetCalculationSql()
            {
                return Expression;
            }
        }

        public class DistinctColumn : MetaColumn
        {
            public DistinctColumn(string? name) : base(name: name)
            {

            }

            public override string TypeName
            {
                get
                {
                    return nameof(DistinctColumn);
                }
            }

            public string GetDistinctSql()
            {
                return $"DISTINCT {Alias}";
            }
        }
    }
}
