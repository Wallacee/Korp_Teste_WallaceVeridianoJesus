using System.Linq.Expressions;
using Korp.Invoice.Inventory.Domain.Entities;
namespace Korp.Invoice.Inventory.Domain.Interfaces;

public interface IBaseRepository<TEntity> where TEntity : BaseEntity
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken =default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken =default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken =default);
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken =default);
    Task<(IReadOnlyCollection<TEntity> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, CancellationToken cancellationToken =default);
}
