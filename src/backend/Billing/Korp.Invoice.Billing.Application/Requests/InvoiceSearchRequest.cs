using Korp.Invoice.Shared.Pagination;

namespace Korp.Invoice.Billing.Application.Requests;

public sealed class InvoiceSearchRequest : PagedRequest
{
    public string? Search { get; init; }
}
