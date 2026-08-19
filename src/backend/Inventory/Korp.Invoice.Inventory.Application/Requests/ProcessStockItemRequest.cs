namespace Korp.Invoice.Inventory.Application.Requests;

public sealed class ProcessStockItemRequest
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}
