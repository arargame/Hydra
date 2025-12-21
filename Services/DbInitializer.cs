using Hydra.DBAccess;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Hydra.Core;

namespace Hydra.Services
{
    public static class DbInitializer
    {
        public static void InitializeAsync<TDbContext>(IServiceProvider serviceProvider, IConfiguration configuration) where TDbContext : DbContext
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logService = services.GetRequiredService<ILogService>();

                // 1. Initialize Log Database (ADO.NET)
                InitializeLogDb(configuration, logService);

                // 2. Initialize Main Database (EF Core)
                try
                {
                    logService.SaveAsync(LogFactory.Info("Startup", "DbInit", "Checking Main Database connection..."), LogRecordType.Console).Wait();
                    
                    var context = services.GetRequiredService<TDbContext>();
                    var created = context.Database.EnsureCreated();
                    
                    logService.SaveAsync(LogFactory.Info("Startup", "DbInit", $"Main Database Initialization: {(created ? "CREATED" : "EXISTING")}"), LogRecordType.Console).Wait();
                }
                catch (Exception ex)
                {
                    logService.SaveAsync(LogFactory.Error($"Main DB Error: {ex.Message}"), LogRecordType.Console).Wait();
                    
                    var logger = services.GetRequiredService<ILogger<DbInitializerLogger>>();
                    logger.LogError(ex, "An error occurred creating the DB.");
                }
            }
        }

        private class DbInitializerLogger { } // Dummy class for Logger category


        public static void InitializeLogDb(IConfiguration configuration, ILogService logService)
        {
            // 1. Get Connection String
            var fullConnectionString = configuration.GetConnectionString("LogDbConnection");
            if (string.IsNullOrEmpty(fullConnectionString)) 
            {
                // Fallback
                fullConnectionString = configuration["LogDbConnection"];
                if(string.IsNullOrEmpty(fullConnectionString)) return;
            }

            try
            {
                var builder = new SqlConnectionStringBuilder(fullConnectionString);
                var targetDatabaseName = builder.InitialCatalog;

                // 2. Connect to MASTER to check/create DB
                builder.InitialCatalog = "master";
                var masterConnectionString = builder.ToString();

                var checkDbQuery = "SELECT COUNT(*) FROM sys.databases WHERE name = @name";
                var parameters = new Dictionary<string, object?> { { "@name", targetDatabaseName } };

                // Check if DB exists
                var existsResult = AdoNetDatabaseService.ExecuteScalar(checkDbQuery, parameters, ConnectionFactory.CreateConnection(ConnectionType.MsSql, masterConnectionString));
                var exists = existsResult != null && (int)existsResult > 0;

                if (!exists)
                {
                    // Create Database
                    var createDbQuery = $"CREATE DATABASE [{targetDatabaseName}]";
                    AdoNetDatabaseService.ExecuteNonQuery(createDbQuery, null, ConnectionFactory.CreateConnection(ConnectionType.MsSql, masterConnectionString));
                }

                // 3. Connect to Target Database to check/create Tables
                
                // Check Log Table
                var checkTableQuery = "SELECT COUNT(*) FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Log]') AND type in (N'U')";
                var tableExistsResult = AdoNetDatabaseService.ExecuteScalar(checkTableQuery, null, ConnectionFactory.CreateConnection(ConnectionType.MsSql, fullConnectionString));
                var tableExists = tableExistsResult != null && (int)tableExistsResult > 0;

                if (!tableExists)
                {
                    var createTableScript = @"
                        CREATE TABLE [dbo].[Log](
                            [Id] [uniqueidentifier] NOT NULL,
                            [Name] [nvarchar](max) NULL,
                            [Description] [nvarchar](max) NULL,
                            [Category] [nvarchar](450) NULL,
                            [EntityName] [nvarchar](max) NULL,
                            [EntityId] [nvarchar](450) NULL,
                            [Type] [nvarchar](50) NOT NULL,
                            [ProcessType] [nvarchar](50) NOT NULL,
                            [AddedDate] [datetime2](7) NOT NULL,
                            [ModifiedDate] [datetime2](7) NOT NULL,
                            [SessionInformationId] [uniqueidentifier] NULL,
                            [Payload] [nvarchar](max) NULL,
                            CONSTRAINT [PK_Log] PRIMARY KEY CLUSTERED ([Id] ASC)
                        )";

                    AdoNetDatabaseService.ExecuteNonQuery(createTableScript, null, ConnectionFactory.CreateConnection(ConnectionType.MsSql, fullConnectionString));

                    // Create Indexes
                    AdoNetDatabaseService.ExecuteNonQuery("CREATE NONCLUSTERED INDEX [IX_Log_EntityId] ON [dbo].[Log] ([EntityId] ASC)", null, ConnectionFactory.CreateConnection(ConnectionType.MsSql, fullConnectionString));
                    AdoNetDatabaseService.ExecuteNonQuery("CREATE NONCLUSTERED INDEX [IX_Log_Category] ON [dbo].[Log] ([Category] ASC)", null, ConnectionFactory.CreateConnection(ConnectionType.MsSql, fullConnectionString));
                    AdoNetDatabaseService.ExecuteNonQuery("CREATE NONCLUSTERED INDEX [IX_Log_SessionInformationId] ON [dbo].[Log] ([SessionInformationId] ASC)", null, ConnectionFactory.CreateConnection(ConnectionType.MsSql, fullConnectionString));
                    
                    logService.SaveAsync(LogFactory.Info("Startup", "DbInit", "Log Database CREATED"), LogRecordType.Console).Wait();
                }
                else
                {
                    logService.SaveAsync(LogFactory.Info("Startup", "DbInit", "Log Database EXISTING"), LogRecordType.Console).Wait();
                }
            }
            catch (Exception ex)
            {
                logService.SaveAsync(LogFactory.Error($"Log DB Error: {ex.Message}"), LogRecordType.Console).Wait();
            }
        }
    }
}
