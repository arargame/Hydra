using Hydra.AccessManagement;
using Hydra.IdentityAndAccess;
using Hydra.Core;
using Hydra.Core.DTOs;
using Hydra.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Hydra.DAL.Contexts
{
    public class HydraDbContext : DbContext
    {

        public HydraDbContext(DbContextOptions options) : base(options)
        {
        }

        protected HydraDbContext(DbContextOptions options, IServiceProvider serviceProvider) 
            : base(options)
        {
        }
        
        // Base constructor for derived contexts to call if they need strictly generic options
        protected HydraDbContext() : base()
        {
        }

        public DbSet<SystemUser> SystemUser { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<Permission> Permission { get; set; }
        public DbSet<RolePermission> RolePermission { get; set; }
        public DbSet<RoleSystemUser> RoleSystemUser { get; set; }
        public DbSet<SystemUserPermission> SystemUserPermission { get; set; }
        //public DbSet<Log> Log { get; set; }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                entityType.SetTableName(entityType.DisplayName());
            }
        }
    }
}
