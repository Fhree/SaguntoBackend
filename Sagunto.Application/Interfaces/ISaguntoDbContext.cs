using Sagunto.Domain.Entities;

namespace Sagunto.Application.Interfaces;

public interface ISaguntoDbContext
{
    Microsoft.EntityFrameworkCore.DbSet<User> Users { get; }
    Microsoft.EntityFrameworkCore.DbSet<Role> Roles { get; }
    Microsoft.EntityFrameworkCore.DbSet<Product> Products { get; }
    Microsoft.EntityFrameworkCore.DbSet<Order> Orders { get; }
    Microsoft.EntityFrameworkCore.DbSet<OrderLine> OrderLines { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}