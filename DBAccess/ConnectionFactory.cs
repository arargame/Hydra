using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DBAccess
{
    public static class ConnectionFactory
    {
        public static CustomConnection CreateConnection(ConnectionType type, string connectionString)
        {
            return type switch
            {
                ConnectionType.Oracle => new OracleConnection(connectionString: connectionString),
                ConnectionType.MsSql => new MsSqlConnection(connectionString: connectionString),
                _ => throw new NotSupportedException($"Unsupported connection type: {type}")
            };
        }
    }
}
