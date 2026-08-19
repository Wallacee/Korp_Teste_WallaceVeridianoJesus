using Korp.Invoice.Inventory.Domain.Repositories;
namespace Korp.Invoice.Inventory.Infrastructure.Persistence;

public sealed class InventoryUnitOfWork : IInventoryUnitOfWork
{
    private readonly InventoryDbContext _context;
    public InventoryUnitOfWork(InventoryDbContext context)
    {
        _context = context;
    }
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    => _context.SaveChangesAsync(cancellationToken);

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
    {
        await using
        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await operation(cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
