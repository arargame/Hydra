using Hydra.Core;
using Hydra.DataModels.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DataModels
{
    public enum JoinType
    {
        Inner,
        Left,
        Right,
        Full
    }

    public interface ITable : IBaseObject<ITable>
    {
        string? Alias { get; set; }

        string? Query { get; set; }

        List<IJoinTable> GetAllJoinTables { get; }

        List<IMetaColumn> GetFilteredMetaColumns { get; }

        List<IMetaColumn> GetSelectedMetaColumnsIncludingJoins { get; }

        List<IMetaColumn> GetFilteredMetaColumnsIncludingJoins { get; }

        List<IMetaColumn> GetOrderedMetaColumnsIncludingJoins { get; }

        Pagination? Pagination { get; set; }

        IQueryableFilter? Filter { get; set; }

        List<IJoinTable> JoinTables { get; set; }

        List<IMetaColumn> MetaColumns { get; set; }

        Dictionary<string, object> QueryParameters { get; set; }

        List<IRow> Rows { get; set; }

        ITable SetAlias(string? alias);

        ITable SetQuery(string query);

        ITable SetPagination(Pagination pagination);

        ITable SetQueryParameters();
    }



    public class Table : BaseObject<Table>, ITable
    {
        public string? Alias { get; set; } = null;

        public string? Query { get; set; } = null;

        public Pagination? Pagination { get; set; } = null;

        public IQueryableFilter? Filter { get; set; } = null;

        public List<IJoinTable> GetAllJoinTables
        {
            get
            {
                var subJoins = new List<IJoinTable>();

                JoinTables.ForEach(jt => JoinTable.GetAllJoinTablesOf(jt, true, ref subJoins));

                return subJoins;
            }
        }

        private List<IMetaColumn> GetMetaColumnsWithIncludesJoins(Func<IMetaColumn, bool> filter)
        {
            return MetaColumns.Where(filter)
                              .Union(GetAllJoinTables.SelectMany(jt => jt.MetaColumns.Where(filter)))
                              .ToList();
        }

        public List<IMetaColumn> GetSelectedMetaColumnsIncludingJoins => GetMetaColumnsWithIncludesJoins(mc => mc.IsSelected);
        public List<IMetaColumn> GetFilteredMetaColumnsIncludingJoins => GetMetaColumnsWithIncludesJoins(mc => mc.IsFiltered);
        public List<IMetaColumn> GetOrderedMetaColumnsIncludingJoins => GetMetaColumnsWithIncludesJoins(mc => mc.IsOrdered);

        public List<IMetaColumn> GetFilteredMetaColumns
        {
            get
            {
                return MetaColumns.Where(mc => mc.IsFiltered)
                                    .Where(mc => (mc.Filter is IsNullFilter || mc.Filter is IsNotNullFilter) || (!mc.Filter.Parameters.Any(p => string.IsNullOrWhiteSpace(p.Value?.ToString()))))
                                    .ToList();
            }
        }

        public List<IJoinTable> JoinTables { get; set; } = new List<IJoinTable>();

        public List<IMetaColumn> MetaColumns { get; set; } = new List<IMetaColumn>();

        public List<IRow> Rows { get; set; } = new List<IRow>();

        public Dictionary<string, object> QueryParameters { get; set; } = new Dictionary<string, object>();

        public Table() { }

        public ITable SetAlias(string? alias)
        {
            if (!string.IsNullOrWhiteSpace(alias) && alias.Contains(' '))
                throw new ArgumentException("Alias cannot contain spaces.");

            Alias = alias;

            return this;
        }

        public ITable SetQuery(string query)
        {
            Query = query;

            return this;
        }

        public ITable SetPagination(Pagination pagination)
        {
            Pagination = pagination;

            return this;
        }

        //public static void GetAllJoinTablesOf(IJoinTable parent, bool includesParent, ref List<IJoinTable> list)
        //{
        //    var stack = new Stack<IJoinTable>();
        //    stack.Push(parent);

        //    while (stack.Any())
        //    {
        //        var current = stack.Pop();

        //        if (current.JoinTables == null || !current.JoinTables.Any())
        //        {
        //            if (includesParent)
        //            {
        //                list.Add(current);
        //            }
        //            continue;
        //        }

        //        if (includesParent)
        //        {
        //            list.Add(current);
        //        }

        //        foreach (var child in current.JoinTables)
        //        {
        //            stack.Push(child);
        //        }
        //    }
        //}

        public static void GetAllJoinTablesOf(IJoinTable parent, bool includesParent, ref List<IJoinTable> list)
        {
            if (parent.JoinTables == null || !parent.JoinTables.Any())
            {
                list.AddRange(new List<IJoinTable>() { parent });

                return;
            }

            if (includesParent)
            {
                list.Add(parent);
            }

            foreach (var child in parent.JoinTables)
            {
                GetAllJoinTablesOf(child, includesParent, ref list);
            }
        }

        public Table SetFilter(IQueryableFilter? filter = null)
        {
            if (filter != null)
                Filter = filter;
            else
            {
                if (GetFilteredMetaColumns.Any())
                {
                    if (GetFilteredMetaColumns.Count == 1)
                        Filter = GetFilteredMetaColumns.First().Filter;
                    else
                        Filter = JoinedFiltersGroup.SetFromColumns(GetFilteredMetaColumns).First();
                }
            }

            return this;
        }

        public ITable SetQueryParameters()
        {
            if (Filter == null)
                return this;

            for (int i = 0; i < Filter.Parameters.Count; i++)
            {
                QueryParameters.Add(string.Format("@{0}", i), Filter.Parameters[i]);
            }

            return this;
        }


    }


}
