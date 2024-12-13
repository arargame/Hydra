using Hydra.DBAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Services
{

    public static class SqlService
    {
        public static bool CheckConnection(CustomConnection? connection = null)
        {
            connection = connection ?? new MsSqlConnection();

            try
            {
                connection.Connect();
                Console.WriteLine("Connected to the database successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}");
                return false;
            }
            finally
            {
                connection.Connection?.Close();
            }
        }

        public static List<Dictionary<string, object?>> ExecuteQuery(string query, Dictionary<string, object>? parameters = null, CustomConnection? connection = null)
        {
            connection = connection ?? new MsSqlConnection();

            using (connection)
            {
                try
                {
                    connection.Connect();
                    var command = connection.CreateCommand();
                    command.CommandText = query;

                    if (parameters != null)
                        command.AddParameters(parameters);

                    var dataReader = command.ExecuteReader();
                    var result = new List<Dictionary<string, object?>>();

                    while (dataReader.Read())
                    {
                        var row = new Dictionary<string, object?>();

                        for (int i = 0; i < dataReader.FieldCount; i++)
                        {
                            var value = dataReader.IsDBNull(i) ? null : dataReader.GetValue(i);

                            row[dataReader.GetName(i)] = value;
                        }
                        
                        result.Add(row);
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error executing query: {ex.Message}");
                    throw;
                }
            }
        }

        public static object? ExecuteScalar(string query, Dictionary<string, object>? parameters = null, CustomConnection? connection = null)
        {
            connection = connection ?? new MsSqlConnection();

            using (connection)
            {
                try
                {
                    connection.Connect();
                    var command = connection.CreateCommand();
                    command.CommandText = query;

                    if (parameters != null)
                        command.AddParameters(parameters);

                    return command.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error executing scalar: {ex.Message}");
                    throw;
                }
            }
        }

        public static int ExecuteNonQuery(string query,
            Dictionary<string, object>? parameters = null,
            CustomConnection? connection = null)
        {
            connection = connection ?? new MsSqlConnection();

            using (connection)
            {
                try
                {
                    connection.Connect();
                    var command = connection.CreateCommand();
                    command.CommandText = query;

                    if (parameters != null)
                        command.AddParameters(parameters);

                    return command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error executing non-query: {ex.Message}");
                    throw;
                }
            }
        }

        public static List<Dictionary<string, object?>> SelectColumnNames(string tableName, CustomConnection? connection = null)
        {
            var query = connection is OracleConnection
                ? $"SELECT COLUMN_NAME, DATA_TYPE FROM USER_TAB_COLUMNS WHERE table_name = '{tableName}'"
                : $"SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'{tableName}'";

            return ExecuteQuery(query, null, connection);
        }

        public static bool Any(string query, Dictionary<string, object>? parameters = null, CustomConnection? connection = null)
        {
            query = $"SELECT 1 FROM ({query}) t";

            var result = ExecuteScalar(query, parameters, connection);

            return result != null && Convert.ToInt32(result) == 1;
        }
        public static bool CreateView(string query, CustomConnection connection)
        {
            string createViewSql;

            // Veritabanı türüne göre SQL cümlesi oluşturuluyor
            if (connection.ConnectionType == ConnectionType.MsSql)
            {
                createViewSql = $"IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'{query}')) " +
                                $"BEGIN ALTER VIEW {query} END ELSE BEGIN CREATE VIEW {query} END";
            }
            else if (connection.ConnectionType == ConnectionType.Oracle)
            {
                createViewSql = $"CREATE OR REPLACE VIEW {query}";
            }
            else
            {
                throw new NotSupportedException($"Unsupported connection type: {connection.ConnectionType}");
            }

            try
            {
                ExecuteNonQuery(query: createViewSql,connection: connection);
                return true; 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating view: {ex.Message}");
                return false; 
            }
        }

    }

}
