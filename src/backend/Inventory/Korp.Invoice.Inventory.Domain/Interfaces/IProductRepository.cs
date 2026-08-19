using Korp.Invoice.Inventory.Domain.Entities;

namespace Korp.Invoice.Inventory.Domain.Interfaces;

public interface IProductRepository : IBaseRepository<Product>
{
    Task<Product?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    Task<(IReadOnlyCollection<Product> Items, int TotalCount)> SearchAsync(
        string? search
        , int page
        , int pageSize
        , string sortBy
        , string sortDirection
        , CancellationToken cancellationToken = default);
}
