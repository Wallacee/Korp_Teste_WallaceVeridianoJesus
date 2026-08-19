using Korp.Invoice.Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Korp.Invoice.Inventory.Infrastructure.Persistence;

public sealed class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
    {
    }
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockOperation> StockOperations => Set<StockOperation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
