namespace Korp.Invoice.Billing.Application.ExternalServices.Inventory;

public sealed record InventoryStockItem(Guid ProductId, int Quantity);
