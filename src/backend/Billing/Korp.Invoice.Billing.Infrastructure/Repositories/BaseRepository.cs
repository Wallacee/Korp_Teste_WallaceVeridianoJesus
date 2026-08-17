using Korp.Invoice.Billing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace Korp.Invoice.Billing.Infrastructure.Repositories;

public abstract class BaseRepository<TEntity> where TEntity : class
{
    protected readonly BillingDbContext Context;
    protected readonly DbSet<TEntity> DbSet;
    protected BaseRepository(BillingDbContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }
    protected async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }
}
