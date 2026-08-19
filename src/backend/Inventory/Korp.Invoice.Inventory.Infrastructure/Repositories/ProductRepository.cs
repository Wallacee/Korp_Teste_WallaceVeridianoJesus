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
    public async Task<(IReadOnlyCollection<Product> Items, int TotalCount)> SearchAsync(string? search, int page, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => EF.Functions.ILike(x.Code, $"%{term}%") || EF.Functions.ILike(x.Description, $"%{term}%"));
        }
        var totalCount = await query.CountAsync(cancellationToken);
        query = (sortBy.ToLowerInvariant(), sortDirection.ToLowerInvariant()) switch
        {
            ("code", "desc") => query.OrderByDescending(x => x.Code),
            ("description", "asc") => query.OrderBy(x => x.Description),
            ("description", "desc") => query.OrderByDescending(x => x.Description),
            ("stock", "asc") => query.OrderBy(x => x.Stock),
            ("stock", "desc") => query.OrderByDescending(x => x.Stock),
            ("createdatutc", "asc") => query.OrderBy(x => x.CreatedAtUtc),
            ("createdatutc", "desc") => query.OrderByDescending(x => x.CreatedAtUtc),
            _ => query.OrderBy(x => x.Code)
        };
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }
}
