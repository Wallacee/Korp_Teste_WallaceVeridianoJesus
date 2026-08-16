using Korp.Invoice.Billing.Domain.Entities;

namespace Korp.Invoice.Billing.Domain.Repositories;

public interface IInvoiceRepository
{
    Task<FiscalInvoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FiscalInvoice?> GetByNumberAsync(long number, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<FiscalInvoice>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(FiscalInvoice invoice, CancellationToken cancellationToken = default);
    Task UpdateAsync(FiscalInvoice invoice, CancellationToken cancellationToken = default);
}
