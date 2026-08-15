namespace Korp.Invoice.Inventory.Application.Requests;

public sealed record CreateProductRequest
{
    public string Code { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Stock { get; init; }
}
