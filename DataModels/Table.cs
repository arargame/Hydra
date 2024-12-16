﻿using Hydra.Core;
using Hydra.DataModels.Filter;
using Hydra.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        int PageNumber { get; set; }

        int PageSize { get; set; }

        List<IJoinTable> JoinTables { get; set; }

        List<IMetaColumn> MetaColumns { get; set; }

        Dictionary<string, object?> QueryParameters { get; set; }

        List<IRow> Rows { get; set; }

        ITable SetAlias(string? alias);

        ITable SetQuery(string query);

        ITable SetPagination(Pagination pagination);

        ITable SetQueryParameters();

        ITable SetPageNumber(int pageNumber);

        ITable SetPageSize(int pageSize);

        ITable SetMetaColumns(params IMetaColumn[] columns);

        ITable SetJoins(Expression<Func<ITable, List<IJoinTable>>> expression);

        ITable SetJoins(params IJoinTable[] joinTables);

        ITable SetFilter(IQueryableFilter? filter = null);
    }

    public class Table : BaseObject<Table>, ITable
    {
        public readonly int DefaultPageNumber = 1;

        public readonly int DefaultPageSize = 10;

        public string? Alias { get; set; } = null;

        public string? Query { get; set; } = null;

        public Pagination? Pagination { get; set; } = null;

        public IQueryableFilter? Filter { get; set; } = null;

        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public Expression<Func<ITable, JoinedFiltersGroup>>? ManageFiltersExpression { get; set; } = null;

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

        public Dictionary<string, object?> QueryParameters { get; set; } = new Dictionary<string, object?>();

        public Table() { }

        public Table(string name, string? alias = null)
        {
            Name = name;

            Alias = alias ?? Name;

            //Type = Helper.GetTypeFromAssembly(typeof(BaseObject), Name);

            PageNumber = DefaultPageNumber;
        }

        public static Table Create(string tableName,
                        string? tableAlias = null,
                List<IMetaColumn>? metaColumns = null,
                Expression<Func<ITable, JoinedFiltersGroup>>? expressionToManageFilters = null,
                Expression<Func<ITable, List<IJoinTable>>>? expressionToSetJoins = null,
                int? pageNumber = null,
                int? pageSize = null)
        {
            var table = new Table(tableName, tableAlias);

            if (metaColumns != null)
                table.SetMetaColumns(metaColumns.ToArray());

            if (expressionToManageFilters != null)
                table.ManageFilters(expressionToManageFilters);

            if (expressionToSetJoins != null)
                table.SetJoins(expressionToSetJoins);

            if (pageNumber != null)
                table.SetPageNumber(pageNumber.Value);

            if (pageSize != null)
                table.SetPageSize(pageSize.Value);

            return table;
        }

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

        public ITable SetFilter(IQueryableFilter? filter = null)
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
                QueryParameters.Add(string.Format("@{0}", i), Filter.Parameters[i].Value);
            }

            return this;
        }

        public ITable SetMetaColumns(params IMetaColumn[] columns)
        {
            MetaColumns.AddRange(columns);

            foreach (var metaColumn in MetaColumns)
            {
                if (metaColumn == null)
                    continue;

                metaColumn.SetTable(this);
            }

            return this;
        }

        public ITable ManageFilters(Expression<Func<ITable, JoinedFiltersGroup>> expression)
        {
            if (expression == null)
                return this;

            ManageFiltersExpression = expression;

            var func = expression.Compile();

            return ManageFiltersWithFunc(expression.Compile());
        }

        public ITable ManageFiltersWithFunc(Func<ITable, JoinedFiltersGroup> func)
        {
            ManageFiltersExpression = t => func(t);

            return SetFilter(func(this));
        }

        public ITable SetPageNumber(int pageNumber)
        {
            PageNumber = pageNumber > 0 ? pageNumber : DefaultPageNumber;

            return this;
        }

        public ITable SetPageSize(int pageSize)
        {
            PageSize = pageSize > 0 ? pageSize : DefaultPageSize;

            return this;
        }
        public ITable SetJoins(Expression<Func<ITable, List<IJoinTable>>> expression)
        {
            var joinTables = expression.Compile().Invoke(this);

            return SetJoins(joinTables.ToArray());
        }

        public ITable SetJoins(params IJoinTable[] joinTables)
        {
            if (joinTables == null)
                return this;

            JoinTables.AddRange(joinTables);

            foreach (var joinTable in joinTables)
            {
                joinTable.SetLeftTable(this);
            }

            return this;
        }

    }


}
