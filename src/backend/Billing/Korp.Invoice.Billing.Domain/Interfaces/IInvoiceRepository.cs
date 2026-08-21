using Korp.Invoice.Billing.Domain.Entities;
using Korp.Invoice.Billing.Domain.Enums;

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
        Task<int> CountByStatusAsync(InvoiceStatus status, CancellationToken cancellationToken = default);
        Task<int> GetProcessedItemsCountAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<(DateTime Date, int Quantity)>> GetDailyConsumptionAsync(int days, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<(Guid ProductId, int Quantity)>> GetTopProductsAsync(int take, CancellationToken cancellationToken = default);
    }
}
