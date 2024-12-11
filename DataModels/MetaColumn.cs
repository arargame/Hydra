using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DataModels
{
    public interface IMetaColumn : IColumn
    {
        bool IsOrderable { get; set; }

        bool IsOrdered { get; set; }

        bool IsSelectable { get; set; }

        bool IsSelected { get; set; }

        bool IsFilterable { get; set; }
        bool IsFiltered { get; set; }

        string TypeName { get; }
    }

    public class MetaColumn : Column, IMetaColumn
    {
        public bool IsOrderable { get; set; }

        public bool IsOrdered { get; set; }

        public bool IsSelectable { get; set; }

        public bool IsSelected { get; set; }

        public bool IsFilterable { get; set; }
        public bool IsFiltered { get; set; }

        public virtual string TypeName
        {
            get
            {
                return GetType().Name;
            }
        }
    }

    public class SelectedColumn : MetaColumn
    {
        public bool GroupBy { get; set; }

        //public SelectedColumn(string name, string alias = null, bool groupBy = false) : base(name, null, alias)
        //{
        //    GroupBy = groupBy;

        //    MakeSelected();
        //}


        public override string TypeName
        {
            get
            {
                return nameof(SelectedColumn);
            }
        }
    }

    public class FilteredColumn : MetaColumn
    {
        //public FilteredColumn(string name, IFilterHasColumn filter) : base(name, filter.GetValue, null)
        //{
        //    MakeFiltered(filter);
        //}

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
        //public OrderedColumn(string name, string direction) : base(name)
        //{
        //    MakeOrdered(direction);
        //}

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

        public AggregatedColumn(string aggregationFunction)
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

        public CalculatedColumn(string expression)
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
