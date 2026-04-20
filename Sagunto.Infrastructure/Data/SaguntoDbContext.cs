using Microsoft.EntityFrameworkCore;
using Sagunto.Domain.Entities;

namespace Sagunto.Infrastructure.Data
{
    public class SaguntoDbContext: DbContext
    {
        public SaguntoDbContext(DbContextOptions<SaguntoDbContext> options) : base(options)
        {
        }
        
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderLine> OrderLines => Set<OrderLine>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.SaguntinoCode).IsUnique();
            });

            
            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(p => p.PriceMember).HasColumnType("decimal(10,2)");
                entity.Property(p => p.PriceGuest).HasColumnType("decimal(10,2)");
            });

            modelBuilder.Entity<OrderLine>(entity =>
            {
                entity.Property(o => o.PriceSnapshot).HasColumnType("decimal(10,2)");
            });
        }
    }
}
