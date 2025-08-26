using Hydra.DataModels.Filter;
using Hydra.DBAccess;
using Hydra.Services;
using Hydra.Utils;
using static Hydra.DataModels.SortingFilterDirectionExtension;

namespace Hydra.DataModels
{
    public class QueryBuilder
    {
        private readonly ITable _table;

        private readonly IDbConnection _connection;

        private string? TopString { get; set; } = null;

        private string? SelectedColumnsString { get; set; } = null;

        private string? FilteredColumnsString { get; set; } = null;

        private string? JoinedTablesString { get; set; } = null;

        private string? QueryToTakeCount { get; set; } = null;

        private int TotalRecordsCount { get; set; }

        private string? QueryToTakeFilteredCount { get; set; } = null;

        private int FilteredTotalRecordsCount { get; set; }

        private string? SelectQuery { get; set; } = null;

        public QueryBuilder(ITable table,IDbConnection connection)
        {
            _table = table;

            _connection = connection;
        }

        public QueryBuilder PrepareFilteredColumnsString()
        {
            FilteredColumnsString = _table.Filter != null ? $"where {_table.Filter.PrepareQueryString()}" : null;

            return this;
        }

        public QueryBuilder PrepareJoinedTablesString()
        {
            JoinedTablesString = "";

            var allJoinTables = _table.GetAllJoinTables;

            foreach (var joinTable in allJoinTables)
            {
                JoinedTablesString += string.Format("{0} join {1} {2} on {3} ", joinTable.JoinType, joinTable.Name, joinTable.Alias, joinTable.Relationship.PrepareQueryString());
            }

            return this; 
        }

        public QueryBuilder PrepareQueryToTakeCount()
        {
            QueryToTakeCount = $"select count(0) from {_table.Name} {_table.Alias} {JoinedTablesString}";

            return this;
        }

        public QueryBuilder ExecuteTotalRecordsCount()
        {
            TotalRecordsCount = (int)((AdoNetDatabaseService.ExecuteScalar(query: QueryToTakeCount, parameters:null,connection: _connection)) ?? 0);

            return this;
        }

        public QueryBuilder BuildTopString()
        {
            TopString = "";

            return this;
        }

        public QueryBuilder PrepareQueryToTakeFilteredCount()
        {
            QueryToTakeFilteredCount = $"select count(0) from {_table.Name} {_table.Alias} {JoinedTablesString} {FilteredColumnsString}";

            return this;
        }

        public QueryBuilder ExecuteFilteredTotalRecordsCount()
        {
            FilteredTotalRecordsCount = (int)(AdoNetDatabaseService.ExecuteScalar(QueryToTakeFilteredCount, _table.QueryParameters, _connection) ?? 0);

            return this;
        }

        public QueryBuilder SetTableFilter()
        {
            if (_table.JoinTables.Any())
            {
                if (_table.GetFilteredMetaColumnsIncludingJoins.Count == 1)
                {
                    _table.SetFilter(_table.GetFilteredMetaColumnsIncludingJoins.FirstOrDefault()?.Filter);
                }
                else
                {
                    var joinFilter = JoinedFiltersGroup.SetFromColumns(_table.GetFilteredMetaColumnsIncludingJoins);

                    _table.SetFilter(joinFilter.FirstOrDefault());
                }
            }
            else
            {
                _table.SetFilter();
            }

            return this;
        }

        public QueryBuilder SetTableQueryParameters()
        {
            if (!_table.QueryParameters.Any() && _table.Filter != null)
                _table.SetQueryParameters();

            return this;
        }

        public QueryBuilder PrepareSelectedColumnsString()
        {
            var selectedColumns = _table.GetSelectedMetaColumnsIncludingJoins
                                               .Where(sc => !sc.IsFileColumn)
                                               .ToList();

            if (!selectedColumns.Any())
            {
                var leftTableSelectedColumnList = AdoNetDatabaseService.SelectColumnNames(_table.Name)
                                                            .Select(c => c["COLUMN_NAME"]?.ToString())
                                                            .Select(columnName => new SelectedColumn(name: columnName, alias: columnName).SetTable(_table))
                                                            .OfType<IMetaColumn>()
                                                            .ToList();

                var joinTablesSelectedColumnList = new List<IMetaColumn>();

                foreach (var joinTable in _table.GetAllJoinTables)
                {
                    joinTablesSelectedColumnList.AddRange(AdoNetDatabaseService.SelectColumnNames(joinTable.Name)
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
                        if (column.Table!=null && column.Table.Name != _table.Name)
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

            var orderByString = PrepareRowNumberOrderString(_table.GetOrderedMetaColumnsIncludingJoins.OfType<OrderedColumn>(), _table.Alias!, _table.HasAnySelectedColumnToGroup);
            SelectedColumnsString += _table.PageSize > 0 ? $",ROW_NUMBER() OVER ({orderByString}) AS RowNumber" : null;


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

                baseQuery = $"select {TopString} {SelectedColumnsString} from {_table.Name} {_table.Alias} {JoinedTablesString} {FilteredColumnsString}";

                if (_table.HasAnySelectedColumnToGroup)
                {
                    baseQuery = GenerateGroupByQuery(_table.GetSelectedMetaColumnsIncludingJoins.OfType<SelectedColumn>(), baseQuery);
                }

                var aliasList = _table.JoinTables.Select(jt => jt.Alias).Append(_table.Alias).Distinct();

                var unusedAlias = Helper.GenerateUnusedCharacterInAWord(string.Join("", aliasList));

                SelectQuery = $"select {unusedAlias}.* from({baseQuery}) {unusedAlias} ";

                _table.SetPageSize(_table.PageSize > 0 ? _table.PageSize : TotalRecordsCount);

                _table.Pagination = new Pagination(_table.PageNumber,
                                                _table.PageSize,
                                                TotalRecordsCount,
                                                FilteredTotalRecordsCount);

                if (_table.PageSize > 0)
                {
                    SelectQuery += $"where {unusedAlias}.RowNumber between {_table.Pagination.Start} and {_table.Pagination.Finish}";
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
            var primaryKey = AdoNetDatabaseService.GetPrimaryKeyName(_table.Name!, _connection); 

            var results = AdoNetDatabaseService.ExecuteQuery(SelectQuery, _table.QueryParameters, _connection);

            foreach (var result in results)
            {
                var columns = result.Select(r => new DataColumn(r.Key, r.Value))
                                    .Where(dc => _table.GetSelectedMetaColumnsIncludingJoins.Any(smc => smc.Alias == dc.Name))
                                    .ToList();

                var row = new Row().SetTable(_table);

                if (result.ContainsKey(primaryKey))
                {
                    row.SetPrimaryKey(result[primaryKey]);
                }

                foreach (var column in columns)
                {
                    row.AddColumn(column);
                }

                _table.AddRow(row);
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
