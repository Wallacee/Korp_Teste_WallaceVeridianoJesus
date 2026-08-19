namespace Korp.Invoice.Billing.Infrastructure.ExternalServices.Inventory;

internal sealed class ProcessStockRequest
{
    public Guid OperationId { get; init; }

    public IReadOnlyCollection<ProcessStockItemRequest> Items { get; init; } = [];
}

internal sealed class ProcessStockItemRequest
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}
