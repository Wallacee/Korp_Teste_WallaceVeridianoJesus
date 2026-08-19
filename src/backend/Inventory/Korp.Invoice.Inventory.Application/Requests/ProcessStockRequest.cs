namespace Korp.Invoice.Inventory.Application.Requests;

public sealed class ProcessStockRequest
{
    public Guid OperationId { get; init; }

    public IReadOnlyCollection<ProcessStockItemRequest> Items { get; init; } = [];
}
