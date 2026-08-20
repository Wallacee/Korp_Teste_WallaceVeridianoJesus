namespace Korp.Invoice.Billing.Domain.Interfaces
{
    public interface IBillingUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default);
    }
}
