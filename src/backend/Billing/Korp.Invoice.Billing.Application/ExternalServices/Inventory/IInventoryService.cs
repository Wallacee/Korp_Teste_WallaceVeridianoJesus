namespace Korp.Invoice.Billing.Application.ExternalServices.Inventory;

public interface IInventoryService
{
    Task<InventoryProductDto?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default);

    Task ProcessStockAsync(Guid operationId, IReadOnlyCollection<InventoryStockItem> items, CancellationToken cancellationToken = default);
}
