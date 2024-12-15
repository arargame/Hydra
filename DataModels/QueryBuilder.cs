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

        private string? FilterString { get; set; } = null;

        private string? JoinString { get; set; } = null;

        private string? QueryToTakeCount { get; set; } = null;

        private string? TopString { get; set; } = null;

        private int TotalRecordsCount { get; set; }

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

        public QueryBuilder PrepareJoinString()
        {
            JoinString = "";

            var allJoinTables = table.GetAllJoinTables;

            foreach (var joinTable in allJoinTables)
            {
                JoinString += string.Format("{0} join {1} {2} on {3} ", joinTable.JoinType, joinTable.Name, joinTable.Alias, joinTable.Relationship.PrepareQueryString());
            }

            return this; 
        }

        public QueryBuilder PrepareQueryToTakeCount()
        {
            QueryToTakeCount = $"select count(0) from {table.Name} {table.Alias} {JoinString}";

            return this;
        }

        public QueryBuilder ExecuteTotalRecordsCount()
        {
            TotalRecordsCount = (int)((DatabaseService.ExecuteScalar(QueryToTakeCount, table.QueryParameters, new MsSqlConnection())) ?? 0);

            return this;
        }

        public QueryBuilder BuildTopString()
        {
            TopString = "";

            return this;
        }


        public QueryBuilder BuildSelectQuery()
        {
            var query = new StringBuilder();


            try 
            {
                BuildTopString();
                PrepareFilterString();
                PrepareJoinString();
                PrepareQueryToTakeCount();


                if (!table.QueryParameters.Any())
                    table.SetQueryParameters();


                ExecuteTotalRecordsCount();


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
