using Korp.Invoice.Inventory.Application.DTOs.Dashboard;

namespace Korp.Invoice.Inventory.Application.Interfaces;

public interface IDashboardAppService
{
    Task<InventorySummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default);
}
