using Hydra.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DataModels.Filter
{
    public enum FilterJoinType
    {
        And,
        Or
    }

    public interface IFilter
    {
        IMetaColumn? Column { get; set; }


        IFilter? RootFilter { get; set; }

        int Priority { get; set; }

        int StartParameterIndex { get; set; }

        bool CreateFilterComponentFromThis { get; set; }

        int FinishParameterIndex { get;}

        object? GetValue { get; }

        List<IFilterParameter> Parameters { get; set; }

        string? PrepareQueryString();

        IFilter SetRootFilter(IFilter filter);

        IFilter SetParameterIndex(int index);

        IFilter SetTable(ITable table);

        IFilter SetPriority(int priority);
        IFilter SetColumn(IMetaColumn column);
        IFilter AddFilterParameter(params IFilterParameter[] filterParameters);
    }
    public class Filter : BaseObject<Filter>, IFilter
    {
        public IMetaColumn? Column { get; set; } = null;

        public IFilter? RootFilter { get; set; } = null;

        public int StartParameterIndex { get; set; }

        public bool CreateFilterComponentFromThis { get; set; }

        public int Priority { get; set; }

        public int FinishParameterIndex
        {
            get
            {
                return StartParameterIndex + Parameters.Count;
            }
        }

        public object? GetValue
        {
            get
            {
                return Parameters != null ? Parameters?.FirstOrDefault()?.Value : null;
            }
        }

        public List<IFilterParameter> Parameters { get; set; } = new List<IFilterParameter>();

        protected Filter() { }


        public Filter(object? value)
        {
            Initialize();

            AddFilterParameter(new FilterParameter(value));
        }


        public Filter(string columnName, object? value, string? alias = null) : this(value)
        {
            SetColumn(new MetaColumn(columnName, value, alias));
        }

        public Filter(IMetaColumn column, object? value) : this(value)
        {
            SetColumn(column);
        }

        public override void Initialize()
        {
            base.Initialize();

            Name = GetType().Name;
        }

        public virtual string? PrepareQueryString()
        {
            return null;
        }

        public IFilter SetTable(ITable table)
        {
            Column?.SetTable(table);

            return this;
        }


        public IFilter SetParameterIndex(int index)
        {
            StartParameterIndex = index;

            for (int i = 0; i < Parameters.Count; i++)
            {
                if (i == 0)
                    Parameters[0].Index = StartParameterIndex;
                else
                    Parameters[i].Index = Parameters[i - 1].Index + 1;
            }

            return this;
        }

        public IFilter SetRootFilter(IFilter filter)
        {
            RootFilter = filter;

            return this;
        }

        public IFilter SetPriority(int priority)
        {
            Priority = priority;

            return this;
        }

        public IFilter SetColumn(IMetaColumn column)
        {
            Column = column;

            return this;
        }

        public IFilter AddFilterParameter(params IFilterParameter[] filterParameters)
        {
            Parameters.AddRange(filterParameters);

            Parameters.ForEach(p => p.SetFilter(this));

            return this;
        }

    }
}
