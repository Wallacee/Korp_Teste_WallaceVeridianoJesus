using System.Linq.Expressions;
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

    public virtual async Task<(IReadOnlyCollection<TEntity> Items, int TotalCount)> GetPagedAsync(
        int page
        , int pageSize
        , Expression<Func<TEntity, bool>>? predicate = null
        , Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null
        , CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = DbSet.AsNoTracking();
        if (predicate is not null)
            query = query.Where(predicate);

        var totalCount = await query.CountAsync(cancellationToken);
        if (orderBy is not null)
            query = orderBy(query);

        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }
}
