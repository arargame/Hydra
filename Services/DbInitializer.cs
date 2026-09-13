using Hydra.DBAccess;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Hydra.Core;
using Hydra.AccessManagement;
using Hydra.IdentityAndAccess;
using System.Linq;

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

                    //Migration tanımlıysa Migrate() kullan: EnsureCreated() veritabanını bir kez oluşturur
                    //ve sonraki migration'lar ASLA uygulanmaz (şema drift → "Invalid column name ..." hataları).
                    var hasMigrations = context.Database.GetMigrations().Any();

                    if (hasMigrations)
                    {
                        var canConnect = context.Database.CanConnect();
                        var hasMigrationHistory = canConnect && context.Database.GetAppliedMigrations().Any();

                        if (canConnect && !hasMigrationHistory)
                        {
                            //DB var ama __EFMigrationsHistory yok → daha önce EnsureCreated ile kurulmuş.
                            //Migrate() bu durumda "table already exists" ile patlar.
                            var recreate = string.Equals(configuration["Database:RecreateOnStartup"], "true", StringComparison.OrdinalIgnoreCase);

                            if (recreate)
                            {
                                logService.SaveAsync(LogFactory.Info("Startup", "DbInit", "RecreateOnStartup=true → dropping and re-creating Main Database via migrations..."), LogRecordType.Console).Wait();

                                context.Database.EnsureDeleted();
                                context.Database.Migrate();

                                logService.SaveAsync(LogFactory.Info("Startup", "DbInit", "Main Database RECREATED via migrations."), LogRecordType.Console).Wait();
                            }
                            else
                            {
                                logService.SaveAsync(LogFactory.Info("Startup", "DbInit",
                                    "WARNING: Database exists WITHOUT migration history (EnsureCreated legacy). " +
                                    "Schema may be out of date. Set \"Database:RecreateOnStartup\": \"true\" in appsettings " +
                                    "(DEV only) to rebuild it via migrations, or update the schema manually."), LogRecordType.Console).Wait();
                            }
                        }
                        else
                        {
                            context.Database.Migrate();

                            logService.SaveAsync(LogFactory.Info("Startup", "DbInit", "Main Database migrations applied (up to date)."), LogRecordType.Console).Wait();
                        }
                    }
                    else
                    {
                        var created = context.Database.EnsureCreated();

                        logService.SaveAsync(LogFactory.Info("Startup", "DbInit", $"Main Database Initialization: {(created ? "CREATED" : "EXISTING")}"), LogRecordType.Console).Wait();
                    }

                    // 3. Initialize Platform Table (Main Database)
                    var mainDbConnectionString = context.Database.GetDbConnection().ConnectionString;
                    InitializePlatformTable(mainDbConnectionString, logService, configuration);

                    // 4. Seed Default Admin (Role "Admin" + SystemUser "admin@<config domain>" + wildcard Permission).
                    // Generic across ANY Hydra-based app (Tentacle today, a future vet/pharmacy/ERP app tomorrow) —
                    // see Hydra Academy "05-Access-Management-Story/05-default-admin-seed.md" for the full design note.
                    SeedDefaultAdmin(context, services, logService);
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

                // Check if old schema exists (EntityName) and Drop if so (Recreate strategy)
                var checkEntityNameCol = "SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Log]') AND name = 'EntityName'";
                var hasEntityName = (int)(AdoNetDatabaseService.ExecuteScalar(checkEntityNameCol, null, ConnectionFactory.CreateConnection(ConnectionType.MsSql, fullConnectionString)) ?? 0) > 0;

                if (hasEntityName)
                {
                     logService.SaveAsync(LogFactory.Warning("Startup", "DbInit", "Old Log Table Schema detected (EntityName). Dropping/Recreating..."), LogRecordType.Console).Wait();
                     AdoNetDatabaseService.ExecuteNonQuery("DROP TABLE [dbo].[Log]", null, ConnectionFactory.CreateConnection(ConnectionType.MsSql, fullConnectionString));
                }

                // Check Log Table (Again, in case it was dropped)
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
                            [EntityType] [nvarchar](max) NULL,
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

        /// <summary>
        /// Generic, config-driven "first boot" seed: creates one Role, one SystemUser and one
        /// wildcard Permission so that ANY Hydra-based application (Tentacle today; a future
        /// veterinary/pharmacy/ERP app tomorrow) can log in and navigate its dashboard the very
        /// first time it runs against an empty database — without any app-specific code.
        ///
        /// Deliberately generic: only SystemUser / Role / Permission (+ the RoleSystemUser /
        /// RolePermission bridge rows) are touched. No Position / Employee / OrganizationUnit —
        /// those are ERP-domain concepts and do not belong in generic Access Management seeding.
        ///
        /// Idempotency gate mirrors InitializePlatformTable's "check the natural key, skip if it
        /// already exists" pattern — but since there is no fixed GUID to check against for a
        /// per-app admin, the check is "does a Role named &lt;Hydra:DefaultAdmin:RoleName&gt; already
        /// exist?". Uses the already-open EF Core `context` (not raw ADO SQL like Platform/Log) so
        /// that the FK wiring between Role/SystemUser/Permission and their bridge rows is handled
        /// by EF's change tracker instead of hand-written multi-table INSERT statements.
        ///
        /// See Hydra Academy "05-Access-Management-Story/05-default-admin-seed.md" for the full
        /// design write-up, including the honest caveat that the wildcard Permission row is a data
        /// scaffold today — no runtime authorization code reads Permission/RolePermission yet.
        /// </summary>
        private static void SeedDefaultAdmin(DbContext context, IServiceProvider services, ILogService logService)
        {
            try
            {
                var configService = services.GetRequiredService<ICustomConfigurationService>();

                var enabledRaw = configService.Get("Hydra:DefaultAdmin:Enabled", "true");
                var enabled = !string.Equals(enabledRaw, "false", StringComparison.OrdinalIgnoreCase);

                if (!enabled)
                {
                    logService.SaveAsync(LogFactory.Info("Startup", "DbInit", "Hydra:DefaultAdmin:Enabled=false. Skipping Default Admin Seed."), LogRecordType.Console).Wait();
                    return;
                }

                // EmailDomain and Password are intentionally NOT defaulted: a wrong-but-valid
                // default (e.g. "example.com" / a hardcoded password shipped in shared library
                // code) is worse than skipping the seed with a clear log message. Every consuming
                // app must set these two explicitly in its own appsettings.
                var emailDomain = configService.Get("Hydra:DefaultAdmin:EmailDomain", string.Empty);
                var password = configService.Get("Hydra:DefaultAdmin:Password", string.Empty);

                if (string.IsNullOrWhiteSpace(emailDomain) || string.IsNullOrWhiteSpace(password))
                {
                    logService.SaveAsync(LogFactory.Warning("Startup", "DbInit",
                        "Hydra:DefaultAdmin:EmailDomain and/or Password not configured. Skipping Default Admin Seed."), LogRecordType.Console).Wait();
                    return;
                }

                var emailLocalPart = configService.Get("Hydra:DefaultAdmin:EmailLocalPart", "admin");
                var roleName = configService.Get("Hydra:DefaultAdmin:RoleName", "Admin");
                var permissionName = configService.Get("Hydra:DefaultAdmin:PermissionName", "*");
                var adminEmail = $"{emailLocalPart}@{emailDomain}";

                // Idempotency gate: has this app already been seeded?
                var alreadySeeded = context.Set<Role>().Any(r => r.Name == roleName);
                if (alreadySeeded)
                {
                    return;
                }

                // Repository<T>.AddAsync (Hydra/DAL/Core/Repository.Command.cs) is the layer that
                // normally stamps AddedDate/ModifiedDate for a new BaseObject; HydraDbContext itself
                // does not do this in SaveChanges. Since this seed calls context.SaveChanges() directly
                // (bypassing Repository<T>) it must stamp both dates itself, the same way, for every
                // row below — otherwise they would persist as DateTime.MinValue.
                var now = DateTime.Now;

                var role = new Role
                {
                    Name = roleName,
                    Description = "Auto-seeded default administrator role (Hydra DbInitializer).",
                    AddedDate = now,
                    ModifiedDate = now
                };

                var permission = new Permission
                {
                    Name = permissionName,
                    Type = PermissionType.ControllerActionBased,
                    Controller = "*",
                    Action = "*",
                    Entity = "*",
                    Property = "*",
                    AllowAnonymous = false,
                    Enabled = true,
                    Description = "Auto-seeded wildcard permission (Hydra DbInitializer). " +
                                  "Data scaffold only — no runtime authorization code checks Permission/RolePermission yet.",
                    AddedDate = now,
                    ModifiedDate = now
                };

                var user = new SystemUser
                {
                    Name = emailLocalPart,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    PasswordHash = PasswordHasher.Hash(password),
                    IsActive = true,
                    Description = "Auto-seeded default administrator user (Hydra DbInitializer).",
                    AddedDate = now,
                    ModifiedDate = now
                };

                var roleSystemUser = new RoleSystemUser { RoleId = role.Id, UserId = user.Id, AddedDate = now, ModifiedDate = now };
                var rolePermission = new RolePermission { RoleId = role.Id, PermissionId = permission.Id, AddedDate = now, ModifiedDate = now };

                context.Set<Role>().Add(role);
                context.Set<Permission>().Add(permission);
                context.Set<SystemUser>().Add(user);
                context.Set<RoleSystemUser>().Add(roleSystemUser);
                context.Set<RolePermission>().Add(rolePermission);

                context.SaveChanges();

                logService.SaveAsync(LogFactory.Info("Startup", "DbInit",
                    $"Seeded Default Admin: {adminEmail} / Role '{roleName}' / Permission '{permissionName}'."), LogRecordType.Console).Wait();
            }
            catch (Exception ex)
            {
                logService.SaveAsync(LogFactory.Error($"Default Admin Seed Error: {ex.Message}"), LogRecordType.Console).Wait();
            }
        }
    }
}
