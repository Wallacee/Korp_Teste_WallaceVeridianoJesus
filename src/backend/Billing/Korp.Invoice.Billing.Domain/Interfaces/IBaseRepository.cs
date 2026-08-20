using System.Linq.Expressions;

namespace Korp.Invoice.Billing.Domain.Interfaces
{
    public interface IBaseRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(Guid id,CancellationToken cancellationToken = default);
        Task AddAsync(TEntity entity,CancellationToken cancellationToken = default);

        Task UpdateAsync(TEntity entity,CancellationToken cancellationToken = default);

        Task DeleteAsync(TEntity entity,CancellationToken cancellationToken = default);

        Task<(IReadOnlyCollection<TEntity> Items, int TotalCount)> GetPagedAsync(
            int page,
            int pageSize,
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            CancellationToken cancellationToken = default);
    }
}
