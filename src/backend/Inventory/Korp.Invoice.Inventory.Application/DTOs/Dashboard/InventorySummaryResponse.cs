namespace Korp.Invoice.Inventory.Application.DTOs.Dashboard;

public sealed record InventorySummaryResponse(int TotalProducts, int TotalStock, int LowStockProducts);
