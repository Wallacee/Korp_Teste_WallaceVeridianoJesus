namespace Korp.Invoice.Billing.Domain.Services;

public interface IInvoiceNumberGenerator
{
    Task<long> GetNextAsync(CancellationToken cancellationToken = default);
}
