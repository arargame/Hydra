using Hydra.DataModels.Filter;
using Hydra.DBAccess;
using Hydra.Services;
using Hydra.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Hydra.DataModels.SortingFilterDirectionExtension;
using static System.Net.Mime.MediaTypeNames;

namespace Hydra.DataModels
{
    public class QueryBuilder
    {
        private readonly ITable table;

        private readonly CustomConnection connection;

        private string? TopString { get; set; } = null;

        private string? SelectedColumnsString { get; set; } = null;

        private string? FilteredColumnsString { get; set; } = null;

        private string? JoinedTablesString { get; set; } = null;

        private string? QueryToTakeCount { get; set; } = null;

        private int TotalRecordsCount { get; set; }

        private string? QueryToTakeFilteredCount { get; set; } = null;

        private int FilteredTotalRecordsCount { get; set; }

        private string? SelectQuery { get; set; } = null;

        public QueryBuilder(ITable table,CustomConnection connection)
        {
            this.table = table;

            this.connection = connection;
        }

        public QueryBuilder PrepareFilteredColumnsString()
        {
            FilteredColumnsString = table.Filter != null ? $"where {table.Filter.PrepareQueryString()}" : null;

            return this;
        }

        public QueryBuilder PrepareJoinedTablesString()
        {
            JoinedTablesString = "";

            var allJoinTables = table.GetAllJoinTables;

            foreach (var joinTable in allJoinTables)
            {
                JoinedTablesString += string.Format("{0} join {1} {2} on {3} ", joinTable.JoinType, joinTable.Name, joinTable.Alias, joinTable.Relationship.PrepareQueryString());
            }

            return this; 
        }

        public QueryBuilder PrepareQueryToTakeCount()
        {
            QueryToTakeCount = $"select count(0) from {table.Name} {table.Alias} {JoinedTablesString}";

            return this;
        }

        public QueryBuilder ExecuteTotalRecordsCount()
        {
            TotalRecordsCount = (int)((DatabaseService.ExecuteScalar(query: QueryToTakeCount, parameters:null,connection: connection)) ?? 0);

            return this;
        }

        public QueryBuilder BuildTopString()
        {
            TopString = "";

            return this;
        }

        public QueryBuilder PrepareQueryToTakeFilteredCount()
        {
            QueryToTakeFilteredCount = $"select count(0) from {table.Name} {table.Alias} {JoinedTablesString} {FilteredColumnsString}";

            return this;
        }

        public QueryBuilder ExecuteFilteredTotalRecordsCount()
        {
            FilteredTotalRecordsCount = (int)(DatabaseService.ExecuteScalar(QueryToTakeFilteredCount, table.QueryParameters, connection) ?? 0);

            return this;
        }

        public QueryBuilder SetTableFilter()
        {
            if (table.JoinTables.Any())
            {
                if (table.GetFilteredMetaColumnsIncludingJoins.Count == 1)
                {
                    table.SetFilter(table.GetFilteredMetaColumnsIncludingJoins.FirstOrDefault()?.Filter);
                }
                else
                {
                    var joinFilter = JoinedFiltersGroup.SetFromColumns(table.GetFilteredMetaColumnsIncludingJoins);

                    table.SetFilter(joinFilter.FirstOrDefault());
                }
            }
            else
            {
                table.SetFilter();
            }

            return this;
        }

        public QueryBuilder SetTableQueryParameters()
        {
            if (!table.QueryParameters.Any() && table.Filter != null)
                table.SetQueryParameters();

            return this;
        }

        public QueryBuilder PrepareSelectedColumnsString()
        {
            var selectedColumns = table.GetSelectedMetaColumnsIncludingJoins
                                               .Where(sc => !sc.IsFileColumn)
                                               .ToList();

            if (!selectedColumns.Any())
            {
                var leftTableSelectedColumnList = DatabaseService.SelectColumnNames(table.Name)
                                                            .Select(c => c["COLUMN_NAME"]?.ToString())
                                                            .Select(columnName => new SelectedColumn(name: columnName, alias: columnName).SetTable(table))
                                                            .OfType<IMetaColumn>()
                                                            .ToList();

                var joinTablesSelectedColumnList = new List<IMetaColumn>();

                foreach (var joinTable in table.GetAllJoinTables)
                {
                    joinTablesSelectedColumnList.AddRange(DatabaseService.SelectColumnNames(joinTable.Name)
                                                .Select(c => c["COLUMN_NAME"]?.ToString())
                                                .Select(columnName => new SelectedColumn(name: columnName, alias: columnName).SetTable(joinTable))
                                                .OfType<IMetaColumn>());
                }

                selectedColumns.AddRange(leftTableSelectedColumnList.Union(joinTablesSelectedColumnList));
            }


            foreach (var group in selectedColumns.GroupBy(sc => sc.Alias))
            {
                if (group.Count() > 1)
                {
                    var columnsToChangeAlias = selectedColumns.Where(sc => sc.Alias == group.Key).ToList();
                    //var columnsToChangeAlias = group.ToList();

                    foreach (var column in columnsToChangeAlias)
                    {
                        if (column.Table!=null && column.Table.Name != table.Name)
                            column.Alias = $"{column.Table?.Name}.{column.Alias}";
                    }
                }
            }
            

            if (!selectedColumns.Any())
            {
                SelectedColumnsString = "*";
            }
            else
            {
                SelectedColumnsString = string.Join(",", selectedColumns.Select(column => $"{column.Table?.Alias}.{column.Name} as [{column.Alias}]"));
            }

            var orderByString = PrepareRowNumberOrderString(table.GetOrderedMetaColumnsIncludingJoins.OfType<OrderedColumn>(), table.Alias!, table.HasAnySelectedColumnToGroup);
            SelectedColumnsString += table.PageSize > 0 ? $",ROW_NUMBER() OVER ({orderByString}) AS RowNumber" : null;


            return this;
        }

        private string PrepareRowNumberOrderString(IEnumerable<OrderedColumn> orderedMetaColumns, string alias, bool hasGroupBy)
        {
            string ResolveAlias(string aliasName) => !hasGroupBy ? aliasName : "grp0";

            if (!orderedMetaColumns.Any())
                return $"ORDER BY {ResolveAlias(alias)}.Id";

            var orderByParts = orderedMetaColumns
                .Select(c => $"{ResolveAlias(c.Table?.Alias!)}.{c.Name} {c.Direction.ConvertToString()}");

            return $"ORDER BY {string.Join(", ", orderByParts)}";
        }

        private string GenerateGroupByQuery(IEnumerable<SelectedColumn> selectedColumns, string baseQuery)
        {
            // GROUP BY yapılacak kolonları seçiyoruz
            var groupByColumns = selectedColumns
                .Where(sc => sc.GroupBy)
                .Select(sc => $"{sc.Table?.Alias}.{sc.Name}")
                .ToList();

            if (!groupByColumns.Any())
                return baseQuery; // Eğer grup kolonları yoksa, gruplama yapmadan sorguyu döneriz.

            // GROUP BY ifadelerini oluşturuyoruz
            var groupByString = string.Join(", ", groupByColumns);

            // İç SELECT sorgusunu oluşturuyoruz
            return $"SELECT grp0.*, ROW_NUMBER() OVER (ORDER BY grp0.Id ASC) AS RowNumber FROM (SELECT {groupByString}, COUNT(*) AS Total FROM ({baseQuery}) grp1 GROUP BY {groupByString}) grp0";
        }


        public QueryBuilder BuildSelectQuery()
        {
            var baseQuery = "";


            try 
            {
                SetTableFilter();

                BuildTopString();
                PrepareFilteredColumnsString();
                PrepareJoinedTablesString();
                PrepareQueryToTakeCount();


                SetTableQueryParameters();


                ExecuteTotalRecordsCount();

                PrepareQueryToTakeFilteredCount();
                ExecuteFilteredTotalRecordsCount();


                PrepareSelectedColumnsString();

                baseQuery = $"select {TopString} {SelectedColumnsString} from {table.Name} {table.Alias} {JoinedTablesString} {FilteredColumnsString}";

                if (table.HasAnySelectedColumnToGroup)
                {
                    baseQuery = GenerateGroupByQuery(table.GetSelectedMetaColumnsIncludingJoins.OfType<SelectedColumn>(), baseQuery);
                }

                var aliasList = table.JoinTables.Select(jt => jt.Alias).Append(table.Alias).Distinct();

                var unusedAlias = Helper.GenerateUnusedCharacterInAWord(string.Join("", aliasList));

                SelectQuery = $"select {unusedAlias}.* from({baseQuery}) {unusedAlias} ";

                table.SetPageSize(table.PageSize > 0 ? table.PageSize : TotalRecordsCount);

                table.Pagination = new Pagination(table.PageNumber,
                                                  table.PageSize,
                                                  TotalRecordsCount,
                                                  FilteredTotalRecordsCount);

                if (table.PageSize > 0)
                {
                    SelectQuery += $"where {unusedAlias}.RowNumber between {table.Pagination.Start} and {table.Pagination.Finish}";
                }


                //SELECT*
                //FROM TableName
                //ORDER BY SomeColumn
                //OFFSET 10 ROWS FETCH NEXT 5 ROWS ONLY;

            }
            catch (Exception ex)
            {
                var message = $"queryToTakeCount:{QueryToTakeCount},queryToTakeFilteredCount:{QueryToTakeFilteredCount}";

                throw;
            }


            //// SELECT kısmı: Seçilen metacolumn'lar
            //query.Append("SELECT ");
            //var selectedColumns = MetaColumns.Where(mc => mc.IsSelected).ToList();
            //for (int i = 0; i < selectedColumns.Count; i++)
            //{
            //    var column = selectedColumns[i];
            //    string columnAlias = column.Alias ?? column.Name;
            //    query.Append($"{column.Name} AS {columnAlias}");
            //    if (i < selectedColumns.Count - 1)
            //        query.Append(", ");
            //}

            //// FROM kısmı: Ana tablo
            //query.Append(" FROM ").Append(TableName);

            //// JOIN işlemleri
            //var joinTables = GetAllJoinTables(); // GetAllJoinTablesOf fonksiyonundan tüm join tabloları al
            //foreach (var joinTable in joinTables)
            //{
            //    query.Append($" JOIN {joinTable.TableName} ON {joinTable.JoinCondition}");
            //}

            //// WHERE kısmı: Filtreler
            //if (Filter != null && Filter.Parameters.Any())
            //{
            //    query.Append(" WHERE ");
            //    for (int i = 0; i < Filter.Parameters.Count; i++)
            //    {
            //        var parameter = Filter.Parameters[i];
            //        query.Append($"{parameter.ColumnName} {parameter.Operator} @{i}");
            //        if (i < Filter.Parameters.Count - 1)
            //            query.Append(" AND ");
            //    }
            //}

            //// ORDER BY kısmı: Sıralama
            //if (OrderedColumns.Any())
            //{
            //    query.Append(" ORDER BY ");
            //    for (int i = 0; i < OrderedColumns.Count; i++)
            //    {
            //        var orderedColumn = OrderedColumns[i];
            //        query.Append($"{orderedColumn.Name} {orderedColumn.SortDirection}");
            //        if (i < OrderedColumns.Count - 1)
            //            query.Append(", ");
            //    }
            //}

            //return query.ToString();

            return this;
        }

        public QueryBuilder SetTableRows()
        {
            var primaryKey = DatabaseService.GetPrimaryKeyName(table.Name!, connection); 

            var results = DatabaseService.ExecuteQuery(SelectQuery, table.QueryParameters, connection);

            foreach (var result in results)
            {
                var columns = result.Select(r => new DataColumn(r.Key, r.Value))
                                    .Where(dc => table.GetSelectedMetaColumnsIncludingJoins.Any(smc => smc.Alias == dc.Name))
                                    .ToList();

                var row = new Row().SetTable(table);

                if (result.ContainsKey(primaryKey))
                {
                    row.SetPrimaryKey(result[primaryKey]);
                }

                foreach (var column in columns)
                {
                    row.AddColumn(column);
                }

                table.AddRow(row);
            }

            return this;
        }


        //public string BuildInsertQuery(Dictionary<string, object> values)
        //{
        //    var columns = string.Join(", ", values.Keys);
        //    var valuesPlaceholders = string.Join(", ", values.Keys.Select(k => $"@{k}"));

        //    return $"INSERT INTO {_tableName} ({columns}) VALUES ({valuesPlaceholders})";
        //}
    }

}
