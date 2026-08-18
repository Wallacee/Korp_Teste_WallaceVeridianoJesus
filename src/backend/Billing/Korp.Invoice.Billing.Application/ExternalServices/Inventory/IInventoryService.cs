namespace Korp.Invoice.Billing.Application.ExternalServices.Inventory;

public interface IInventoryService
{
    Task<InventoryProductDto?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default);

    Task DebitStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);
}
