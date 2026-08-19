using Korp.Invoice.Inventory.Domain.Entities;
using Korp.Invoice.Inventory.Domain.Repositories;
using Korp.Invoice.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace Korp.Invoice.Inventory.Infrastructure.Repositories;

public sealed class StockOperationRepository : IStockOperationRepository
{
    private readonly InventoryDbContext _context;
    public StockOperationRepository(InventoryDbContext context)
    {
        _context = context;
    }
    public Task<bool> ExistsAsync(Guid operationId, CancellationToken cancellationToken = default)
     => _context.Set<StockOperation>().AnyAsync(x => x.OperationId == operationId, cancellationToken);

    public async Task AddAsync(StockOperation operation, CancellationToken cancellationToken = default)
    => await _context.Set<StockOperation>().AddAsync(operation, cancellationToken);

}
