using Korp.Invoice.Billing.Domain.Interfaces;
using Korp.Invoice.Billing.Infrastructure.Persistence;
namespace Korp.Invoice.Billing.Infrastructure;

public sealed class BillingUnitOfWork : IBillingUnitOfWork
{
    private readonly BillingDbContext _context;
    public BillingUnitOfWork(BillingDbContext context)
    {
        _context = context;
    }
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken =default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken =default)
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
