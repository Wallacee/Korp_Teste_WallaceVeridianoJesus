using System.Linq.Expressions;
using Korp.Invoice.Inventory.Domain.Entities;
using Korp.Invoice.Inventory.Domain.Interfaces;
using Korp.Invoice.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace Korp.Invoice.Inventory.Infrastructure.Repositories;

public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly InventoryDbContext Context;
    protected readonly DbSet<TEntity> DbSet;
    protected BaseRepository(InventoryDbContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }
    public virtual Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    => DbSet.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
    }
    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbSet.Update(entity);
        await Context.SaveChangesAsync(cancellationToken);
    }
    public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbSet.Remove(entity);
        await Context.SaveChangesAsync(cancellationToken);
    }
    public virtual async Task<(IReadOnlyCollection<TEntity> Items, int TotalCount)>
        GetPagedAsync(
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

        query = orderBy is not null ? orderBy(query) : query.OrderBy(x => x.Id);

        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
