using Korp.Invoice.Billing.Domain.Entities;

namespace Korp.Invoice.Billing.Domain.Interfaces
{
    public interface IInvoiceRepository : IBaseRepository<FiscalInvoice>
    {
        Task<FiscalInvoice?> GetByNumberAsync(long number, CancellationToken cancellationToken = default);
        Task<(IReadOnlyCollection<FiscalInvoice> Items, int TotalCount)> SearchAsync(
            string? search,
            int page,
            int pageSize,
            string sortBy,
            string sortDirection,
            CancellationToken cancellationToken = default);

        Task<bool> HasProductAsync(Guid productId, CancellationToken cancellationToken = default);
    }
}
