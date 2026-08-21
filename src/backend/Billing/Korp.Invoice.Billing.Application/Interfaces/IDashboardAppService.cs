using Korp.Invoice.Billing.Application.DTOs.Dashboard;

namespace Korp.Invoice.Billing.Application.Interfaces;

public interface IDashboardAppService
{
    Task<DashboardSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<DailyConsumptionResponse>> GetDailyConsumptionAsync(int days, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TopProductResponse>> GetTopProductsAsync(int take, CancellationToken cancellationToken = default);
}
