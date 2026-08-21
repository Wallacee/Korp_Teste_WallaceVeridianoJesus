using System.Linq.Expressions;
using Korp.Invoice.Inventory.Domain.Entities;
using Korp.Invoice.Inventory.Domain.Interfaces;
using Korp.Invoice.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace Korp.Invoice.Inventory.Infrastructure.Repositories;

public sealed class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(InventoryDbContext context) : base(context) { }
    public Task<Product?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    => DbSet.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
    public async Task<IReadOnlyCollection<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var productIds = ids.Distinct().ToList();
        return await DbSet.Where(x => productIds.Contains(x.Id)).ToListAsync(cancellationToken);
    }
    public Task<(IReadOnlyCollection<Product> Items, int TotalCount)> SearchAsync(
     string? search = null,
     int page = 1,
     int pageSize = 10,
     string? sortBy = null,
     string? sortDirection = null,
     CancellationToken cancellationToken = default)
    {
        Expression<Func<Product, bool>>? predicate = null;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            predicate = x =>
                x.Code.Contains(term) ||
                x.Description.Contains(term);
        }

        var normalizedSortBy = sortBy?.Trim().ToLowerInvariant() ?? "code";
        var normalizedDirection = sortDirection?.Trim().ToLowerInvariant() ?? "asc";

        Func<IQueryable<Product>, IOrderedQueryable<Product>> orderBy =
            (normalizedSortBy, normalizedDirection) switch
            {
                ("code", "asc") => query => query.OrderBy(x => x.Code),
                ("code", "desc") => query => query.OrderByDescending(x => x.Code),

                ("description", "asc") => query => query.OrderBy(x => x.Description),
                ("description", "desc") => query => query.OrderByDescending(x => x.Description),

                ("stock", "asc") => query => query.OrderBy(x => x.Stock),
                ("stock", "desc") => query => query.OrderByDescending(x => x.Stock),

                ("createdatutc", "asc") => query => query.OrderBy(x => x.CreatedAtUtc),
                ("createdatutc", "desc") => query => query.OrderByDescending(x => x.CreatedAtUtc),

                _ => query => query.OrderBy(x => x.Code)
            };

        return GetPagedAsync(
            page,
            pageSize,
            predicate,
            orderBy,
            cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
       => await DbSet.AsNoTracking().CountAsync(cancellationToken);

    public async Task<int> GetTotalStockAsync(CancellationToken cancellationToken = default)
    => await DbSet.AsNoTracking().SumAsync(product => product.Stock, cancellationToken);

    public async Task<int> CountLowStockAsync(int threshold, CancellationToken cancellationToken = default)
    => await DbSet.AsNoTracking().CountAsync(product => product.Stock <= threshold, cancellationToken);

}
