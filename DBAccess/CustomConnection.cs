using Microsoft.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;

namespace Hydra.DBAccess
{
    public enum ConnectionType
    {
        Oracle,
        MsSql,
        PostgreSql
    }

    public interface IDbConnection
    {
        ConnectionType ConnectionType { get; set; }

        string ConnectionString { get; set; }

        DbConnection Connection { get; set; }

        IDbConnection SetConnectionString(string connectionString);

        IDbConnection SetConnection(DbConnection connection);

        IDbConnection Connect();

        IDbConnection Disconnect();
    }

    public abstract class CustomConnection : IDbConnection, IDisposable
    {
        public CustomConnection(string provider, string connectionString, ConnectionType connectionType)
        {
            if (string.IsNullOrWhiteSpace(provider))
                throw new ArgumentException("Provider cannot be null or empty.", nameof(provider));

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));

            ConnectionType = connectionType;

            SetConnectionString(connectionString);
            InitializeFactory(provider);
            InitializeConnection(provider);
        }

        private void InitializeFactory(string provider)
        {
            switch (ConnectionType)
            {
                case ConnectionType.Oracle:
                    DbProviderFactories.RegisterFactory(provider, OracleClientFactory.Instance);
                    break;
                case ConnectionType.MsSql:
                    DbProviderFactories.RegisterFactory(provider, SqlClientFactory.Instance);
                    break;
                //case ConnectionType.PostgreSql:
                //    DbProviderFactories.RegisterFactory(provider, Npgsql.NpgsqlFactory.Instance);
                //    break;
                default:
                    throw new NotSupportedException($"Unsupported connection type: {ConnectionType}");
            }
        }

        private void InitializeConnection(string provider)
        {
            var factory = DbProviderFactories.GetFactory(provider);

            if (factory == null)
                throw new InvalidOperationException($"No factory found for provider: {provider}");

            _connection = factory.CreateConnection();

            if (Connection == null)
                throw new InvalidOperationException("Failed to create a connection object.");

            Connection.ConnectionString = ConnectionString;
        }

        public static ConcurrentDictionary<string, string> ConnectionStringPool { get; set; } = new();

        public ConnectionType ConnectionType { get; set; }

        public string ConnectionString { get; set; } = "";

        private DbConnection? _connection;

        public DbConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    throw new InvalidOperationException("Connection is not initialized.");
                }

                return _connection;
            }
            set
            {
                _connection = value;
            }
        }

        public static void AddConnectionString(string key, string value)
        {
            ConnectionStringPool[key] = value;
        }

        public static string GetConnectionString(string connectionStringName)
        {
            if (ConnectionStringPool.TryGetValue(connectionStringName, out var connectionString))
            {
                return connectionString;
            }

            throw new KeyNotFoundException($"Connection string '{connectionStringName}' not found in the pool.");
        }

        public IDbConnection SetConnectionStringByName(string connectionStringName)
        {
            try
            {
                ConnectionString = GetConnectionString(connectionStringName);
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"Connection string error: {ex.Message}");
                throw; 
            }
            return this;
        }

        public IDbConnection SetConnectionString(string connectionString)
        {
            ConnectionString = connectionString;

            return this;
        }

        public IDbConnection SetConnection(DbConnection connection)
        {
            Connection = connection;

            return this;
        }

        public IDbConnection Connect()
        {
            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }

            return this;
        }

        public IDbConnection Disconnect()
        {
            if (Connection.State != ConnectionState.Closed)
            {
                Connection.Close();
            }

            return this;
        }

        public IDbCommand CreateCommand()
        {
            return Connection.CreateCommand();
        }

        public static void Start(Func<ConcurrentDictionary<string, string>> connectionStringsProvider)
        {
            ConnectionStringPool = connectionStringsProvider();
        }


        public void Dispose()
        {
            if (Connection?.State == ConnectionState.Open)
            {
                Disconnect();
            }

            //Connection?.Dispose();
        }
    }

    public class MsSqlConnection : CustomConnection
    {
        public MsSqlConnection(string provider = "System.Data.SqlClient", string? connectionString = null, ConnectionType connectionType = ConnectionType.MsSql)
            : base(provider, connectionString ?? GetConnectionString("DefaultConnection"), connectionType)
        {
        }
    }

    public class OracleConnection : CustomConnection
    {
        public OracleConnection(string provider = "Oracle.DataAccess.Client", string? connectionString = null, ConnectionType connectionType = ConnectionType.Oracle)
            : base(provider, connectionString ?? GetConnectionString("OracleConnectionString"), connectionType)
        {
        }
    }

    public class PostgreSqlConnection : CustomConnection
    {
        public PostgreSqlConnection(string provider = "Npgsql", string connectionString = "PostgresConnection", ConnectionType connectionType = ConnectionType.PostgreSql)
            : base(provider, connectionString, connectionType)
        {
        }
    }
}
