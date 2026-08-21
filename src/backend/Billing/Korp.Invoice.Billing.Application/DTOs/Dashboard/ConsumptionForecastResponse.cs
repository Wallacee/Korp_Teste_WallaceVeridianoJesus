namespace Korp.Invoice.Billing.Application.DTOs.Dashboard;

public sealed record ConsumptionForecastResponse(
    bool HasEnoughData,
    int HistoricalDays,
    int ForecastDays,
    double DailyAverage,
    int EstimatedConsumption,
    string Trend,
    IReadOnlyCollection<ConsumptionForecastPointResponse> Forecast);

public sealed record ConsumptionForecastPointResponse(DateTime Date, int EstimatedQuantity);
