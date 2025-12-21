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

                    // 3. Initialize Platform Table (Main Database)
                    var mainDbConnectionString = context.Database.GetDbConnection().ConnectionString;
                    InitializePlatformTable(mainDbConnectionString, logService, configuration);
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
                            [CorrelationId] [uniqueidentifier] NULL,
                            [PlatformId] [uniqueidentifier] NULL,
                            CONSTRAINT [PK_Log] PRIMARY KEY CLUSTERED ([Id] ASC)
                        )";

                    AdoNetDatabaseService.ExecuteNonQuery(createTableScript, null, ConnectionFactory.CreateConnection(ConnectionType.MsSql, fullConnectionString));

                    // Create Indexes
                    AdoNetDatabaseService.ExecuteNonQuery("CREATE NONCLUSTERED INDEX [IX_Log_EntityId] ON [dbo].[Log] ([EntityId] ASC)", null, ConnectionFactory.CreateConnection(ConnectionType.MsSql, fullConnectionString));
                    AdoNetDatabaseService.ExecuteNonQuery("CREATE NONCLUSTERED INDEX [IX_Log_Category] ON [dbo].[Log] ([Category] ASC)", null, ConnectionFactory.CreateConnection(ConnectionType.MsSql, fullConnectionString));
                    AdoNetDatabaseService.ExecuteNonQuery("CREATE NONCLUSTERED INDEX [IX_Log_SessionInformationId] ON [dbo].[Log] ([SessionInformationId] ASC)", null, ConnectionFactory.CreateConnection(ConnectionType.MsSql, fullConnectionString));
                     AdoNetDatabaseService.ExecuteNonQuery("CREATE NONCLUSTERED INDEX [IX_Log_CorrelationId] ON [dbo].[Log] ([CorrelationId] ASC)", null, ConnectionFactory.CreateConnection(ConnectionType.MsSql, fullConnectionString));
                    AdoNetDatabaseService.ExecuteNonQuery("CREATE NONCLUSTERED INDEX [IX_Log_PlatformId] ON [dbo].[Log] ([PlatformId] ASC)", null, ConnectionFactory.CreateConnection(ConnectionType.MsSql, fullConnectionString));

                    logService.SaveAsync(LogFactory.Info("Startup", "DbInit", "Log Database CREATED"), LogRecordType.Console).Wait();
                }
                else
                {
                    LogFactory.Info("Startup", "DbInit", "Log Database EXISTING, Checking Schema...");

                    // Check for missing columns (Schema Migration Lite)
                    var checkCorrelationCol = "SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Log]') AND name = 'CorrelationId'";
                    var hasCorrelation = (int)(AdoNetDatabaseService.ExecuteScalar(checkCorrelationCol, null, ConnectionFactory.CreateConnection(ConnectionType.MsSql, fullConnectionString)) ?? 0) > 0;

                    if (!hasCorrelation)
                    {
                         AdoNetDatabaseService.ExecuteNonQuery("ALTER TABLE [dbo].[Log] ADD [CorrelationId] [uniqueidentifier] NULL", null, ConnectionFactory.CreateConnection(ConnectionType.MsSql, fullConnectionString));
                         AdoNetDatabaseService.ExecuteNonQuery("CREATE NONCLUSTERED INDEX [IX_Log_CorrelationId] ON [dbo].[Log] ([CorrelationId] ASC)", null, ConnectionFactory.CreateConnection(ConnectionType.MsSql, fullConnectionString));
                         logService.SaveAsync(LogFactory.Info("Startup", "DbInit", "Log Table ALTERED (Added CorrelationId)"), LogRecordType.Console).Wait();
                    }

                    var checkPlatformCol = "SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Log]') AND name = 'PlatformId'";
                    var hasPlatform = (int)(AdoNetDatabaseService.ExecuteScalar(checkPlatformCol, null, ConnectionFactory.CreateConnection(ConnectionType.MsSql, fullConnectionString)) ?? 0) > 0;

                    if (!hasPlatform)
                    {
                        AdoNetDatabaseService.ExecuteNonQuery("ALTER TABLE [dbo].[Log] ADD [PlatformId] [uniqueidentifier] NULL", null, ConnectionFactory.CreateConnection(ConnectionType.MsSql, fullConnectionString));
                        AdoNetDatabaseService.ExecuteNonQuery("CREATE NONCLUSTERED INDEX [IX_Log_PlatformId] ON [dbo].[Log] ([PlatformId] ASC)", null, ConnectionFactory.CreateConnection(ConnectionType.MsSql, fullConnectionString));
                        logService.SaveAsync(LogFactory.Info("Startup", "DbInit", "Log Table ALTERED (Added PlatformId)"), LogRecordType.Console).Wait();
                    }
                }

                // FK Removed from here (Platform moved to Main DB)

            }
            catch (Exception ex)
            {
                logService.SaveAsync(LogFactory.Error($"Log DB Error: {ex.Message}"), LogRecordType.Console).Wait();
            }
        }


        private static void InitializePlatformTable(string connectionString, ILogService logService, IConfiguration configuration)
        {
            try
            {
                // Platform is now created by EF Core (EnsureCreated) because it's in HydraDbContext.
                // We just need to SEED it if it's empty.

                // Get ID from Config
                var platformIdString = configuration["Hydra:PlatformId"];
                if (string.IsNullOrEmpty(platformIdString) || !Guid.TryParse(platformIdString, out var platformId))
                {
                     logService.SaveAsync(LogFactory.Warning("Startup", "DbInit", "Hydra:PlatformId not found/valid in Config. Skipping Platform Seed."), LogRecordType.Console).Wait();
                     return;
                }

                var checkSeedQuery = "SELECT COUNT(*) FROM [dbo].[Platform] WHERE Id = @Id";
                var seedExists = (int)(AdoNetDatabaseService.ExecuteScalar(checkSeedQuery, new Dictionary<string, object?> { { "@Id", platformId } }, ConnectionFactory.CreateConnection(ConnectionType.MsSql, connectionString)) ?? 0) > 0;

                if (!seedExists)
                {
                    // Get Project Name dynamically
                    var projectName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "Unknown Service";

                    var insertSeedQuery = @"
                        INSERT INTO [dbo].[Platform] ([Id], [Name], [Description], [ProjectType], [FrameworkVersion], [AddedDate], [ModifiedDate], [IsActive])
                        VALUES (@Id, @Name, @Description, @ProjectType, @FrameworkVersion, @AddedDate, @ModifiedDate, @IsActive)";

                    var parameters = new Dictionary<string, object?>
                    {
                        { "@Id", platformId },
                        { "@Name", projectName }, 
                        { "@Description", "Auto-Seeded Service from Config" },
                        { "@ProjectType", 1 }, // Default to WebApi
                        { "@FrameworkVersion", ".NET Core" },
                        { "@AddedDate", DateTime.UtcNow },
                        { "@ModifiedDate", DateTime.UtcNow },
                        { "@IsActive", true }
                    };

                    AdoNetDatabaseService.ExecuteNonQuery(insertSeedQuery, parameters, ConnectionFactory.CreateConnection(ConnectionType.MsSql, connectionString));
                    logService.SaveAsync(LogFactory.Info("Startup", "DbInit", $"Seeded Platform: {platformId} ({projectName}) in Main DB"), LogRecordType.Console).Wait();
                }
            }
            catch (Exception ex)
            {
                logService.SaveAsync(LogFactory.Error($"Platform DB Error: {ex.Message}"), LogRecordType.Console).Wait();
            }
        }
    }
}
