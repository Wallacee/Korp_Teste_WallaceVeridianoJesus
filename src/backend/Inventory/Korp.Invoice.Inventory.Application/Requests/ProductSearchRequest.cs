namespace Korp.Invoice.Inventory.Application.Requests;

public sealed class ProductSearchRequest
{
    public string? Search { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public string SortBy { get; init; } = "code";

    public string SortDirection { get; init; } = "asc";
}
