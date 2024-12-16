using Hydra.DataModels.Filter;
using Hydra.DBAccess;
using Hydra.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DataModels
{
    public class QueryBuilder
    {
        private readonly ITable table;

        private readonly CustomConnection connection;

        private string? SelectedColumnsString { get; set; } = null;

        private string? FilterString { get; set; } = null;

        private string? JoinedTablesString { get; set; } = null;

        private string? QueryToTakeCount { get; set; } = null;

        private string? TopString { get; set; } = null;

        private int TotalRecordsCount { get; set; }

        private string? QueryToTakeFilteredCount { get; set; } = null;

        private int FilteredTotalRecordsCount { get; set; }

        public QueryBuilder(ITable table,CustomConnection connection)
        {
            this.table = table;

            this.connection = connection;
        }

        public QueryBuilder PrepareFilterString()
        {
            FilterString = table.Filter != null ? $"where {table.Filter.PrepareQueryString()}" : null;

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
            QueryToTakeFilteredCount = $"select count(0) from {table.Name} {table.Alias} {JoinedTablesString} {FilterString}";

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
                var leftTableSelectedColumnList = (IEnumerable<IMetaColumn>)DatabaseService.SelectColumnNames(table.Name)
                                                            .Select(c => c["COLUMN_NAME"]?.ToString())
                                                            .Select(columnName => new SelectedColumn(name: columnName,alias:columnName ).SetTable(table));

                var joinTablesSelectedColumnList = new List<IMetaColumn>();

                foreach (var joinTable in table.GetAllJoinTables)
                {
                    joinTablesSelectedColumnList.AddRange((IEnumerable<IMetaColumn>)DatabaseService.SelectColumnNames(joinTable.Name)
                                                .Select(c => c["COLUMN_NAME"]?.ToString())
                                                .Select(columnName => new SelectedColumn(name: columnName, alias: columnName)
                                                .SetTable(joinTable)));
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

            return this;
        }

        public QueryBuilder BuildSelectQuery()
        {
            var query = "";


            try 
            {
                SetTableFilter();

                BuildTopString();
                PrepareFilterString();
                PrepareJoinedTablesString();
                PrepareQueryToTakeCount();


                SetTableQueryParameters();


                ExecuteTotalRecordsCount();

                PrepareQueryToTakeFilteredCount();
                ExecuteFilteredTotalRecordsCount();


                PrepareSelectedColumnsString();

                query = $"select {TopString} {SelectedColumnsString} from {table.Name} {table.Alias} {JoinedTablesString} {FilterString}";

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


        //public string BuildInsertQuery(Dictionary<string, object> values)
        //{
        //    var columns = string.Join(", ", values.Keys);
        //    var valuesPlaceholders = string.Join(", ", values.Keys.Select(k => $"@{k}"));

        //    return $"INSERT INTO {_tableName} ({columns}) VALUES ({valuesPlaceholders})";
        //}
    }

}
