using Korp.Invoice.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace Korp.Invoice.Inventory.Infrastructure.Repositories;

public abstract class BaseRepository<TEntity> where TEntity : class
{
    protected readonly InventoryDbContext Context;
    protected readonly DbSet<TEntity> DbSet;
    protected BaseRepository(InventoryDbContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }
    protected async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }
}
