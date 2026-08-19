using Korp.Invoice.Inventory.Domain.Entities;

namespace Korp.Invoice.Inventory.Domain.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Product?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<(IReadOnlyCollection<Product> Items, int TotalCount)> SearchAsync(string? search, int page, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
}
