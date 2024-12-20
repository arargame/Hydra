using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DBAccess
{
    public static class DbCommanExtension
    {
        public static void AddParameters(this IDbCommand command, Dictionary<string, object?> parameters)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (parameters == null || parameters.Count == 0)
                return;

            if (command is OracleCommand oracleCommand)
            {
                oracleCommand.BindByName = true;
            }

            foreach (var pair in parameters)
            {
                var parameter = command.CreateParameter();

                parameter.ParameterName = FormatParameterName(command, pair.Key);
                parameter.Value = pair.Value != null && !string.IsNullOrEmpty(pair.Value.ToString()) ? pair.Value : DBNull.Value;

                command.Parameters.Add(parameter);
            }
        }

        private static string FormatParameterName(IDbCommand command, string key)
        {
            return command is OracleCommand ? $":{key.TrimStart('@')}" : $"@{key.TrimStart('@')}";
        }

        private static DbType GetDbType(object value)
        {
            return value switch
            {
                int => DbType.Int32,
                long => DbType.Int64,
                string => DbType.String,
                DateTime => DbType.DateTime,
                bool => DbType.Boolean,
                _ => DbType.Object
            };
        }
    }
}
