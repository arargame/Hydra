using Hydra.AccessManagement;
using Hydra.IdentityAndAccess;
using Hydra.Core;
using Hydra.Core.HumanResources;
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
        
        // HR
        public DbSet<OrganizationUnit> OrganizationUnit { get; set; }
        public DbSet<Position> Position { get; set; }
        public DbSet<Employee> Employee { get; set; }
        public DbSet<Platform> Platform { get; set; }
        
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

            // Human Resources Configuration
            modelBuilder.Entity<OrganizationUnit>()
                .HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrganizationUnit>()
                .HasMany(u => u.Positions)
                .WithOne(p => p.OrganizationUnit)
                .HasForeignKey(p => p.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrganizationUnit>()
                .HasOne(u => u.ManagerPosition)
                .WithMany() // ManagerPosition is just a Position, no specific inverse navigation for "ManagesUnit"
                .HasForeignKey(u => u.ManagerPositionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Position>()
                .HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Position)
                .WithMany(p => p.Employees)
                .HasForeignKey(e => e.PositionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
