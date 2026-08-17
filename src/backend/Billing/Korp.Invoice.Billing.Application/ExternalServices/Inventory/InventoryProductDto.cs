namespace Korp.Invoice.Billing.Application.ExternalServices.Inventory;

public sealed class InventoryProductDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Stock { get; init; }
}
