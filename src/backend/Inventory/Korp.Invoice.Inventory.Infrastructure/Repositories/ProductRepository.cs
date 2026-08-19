using Korp.Invoice.Inventory.Domain.Entities;
using Korp.Invoice.Inventory.Domain.Repositories;
using Korp.Invoice.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace Korp.Invoice.Inventory.Infrastructure.Repositories;

public sealed class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(InventoryDbContext context) : base(context) { }
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    => await DbSet.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<Product?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    => await DbSet.AsNoTracking().FirstOrDefaultAsync(x => x.Code == code, cancellationToken);

    public async Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    => await DbSet.AsNoTracking().OrderBy(x => x.Description).ToListAsync(cancellationToken);

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await base.AddAsync(product, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
    }
    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        DbSet.Update(product);
        await Context.SaveChangesAsync(cancellationToken);
    }
    public async Task<IReadOnlyCollection<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var productIds = ids.Distinct().ToList();

        return await DbSet.Where(x => productIds.Contains(x.Id)).ToListAsync(cancellationToken);
    }
}
