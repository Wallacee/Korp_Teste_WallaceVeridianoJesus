using Korp.Invoice.Billing.Application.DTOs.Dashboard;

namespace Korp.Invoice.Billing.Application.Interfaces;

public interface IDashboardAppService
{
    Task<DashboardSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<DailyConsumptionResponse>> GetDailyConsumptionAsync(int days, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TopProductResponse>> GetTopProductsAsync(int take, CancellationToken cancellationToken = default);
    Task<ConsumptionForecastResponse> GetConsumptionForecastAsync(int historicalDays = 30, int forecastDays = 7, CancellationToken cancellationToken = default);
}
