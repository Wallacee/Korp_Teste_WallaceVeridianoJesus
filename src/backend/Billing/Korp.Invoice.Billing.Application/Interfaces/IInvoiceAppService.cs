using Korp.Invoice.Billing.Application.DTOs;
using Korp.Invoice.Billing.Application.Requests;
using Korp.Invoice.Shared.Pagination;

namespace Korp.Invoice.Billing.Application.Interfaces;

public interface IInvoiceAppService
{
    Task<FiscalInvoiceDto> CreateAsync(CreateInvoiceRequest request, CancellationToken cancellationToken = default);

    Task<FiscalInvoiceDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<FiscalInvoiceDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<FiscalInvoiceDto> ProcessAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<FiscalInvoiceDto>> SearchAsync(InvoiceSearchRequest request, CancellationToken cancellationToken = default);
}
