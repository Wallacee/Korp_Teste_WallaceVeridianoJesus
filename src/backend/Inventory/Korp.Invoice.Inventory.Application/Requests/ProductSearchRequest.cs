using Korp.Invoice.Shared.Pagination;

namespace Korp.Invoice.Inventory.Application.Requests;

public sealed class ProductSearchRequest : PagedRequest
{
    public string? Search { get; init; }
}
