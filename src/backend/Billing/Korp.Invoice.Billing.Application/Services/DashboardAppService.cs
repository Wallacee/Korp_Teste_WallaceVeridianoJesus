using Korp.Invoice.Billing.Application.DTOs.Dashboard;
using Korp.Invoice.Billing.Application.ExternalServices.AI;
using Korp.Invoice.Billing.Application.Interfaces;
using Korp.Invoice.Billing.Domain.Enums;
using Korp.Invoice.Billing.Domain.Interfaces;
namespace Korp.Invoice.Billing.Application.Services;

public sealed class DashboardAppService : IDashboardAppService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IConsumptionForecastService _forecastService;
    public DashboardAppService(IInvoiceRepository invoiceRepository, IConsumptionForecastService forecastService)
    {
        _invoiceRepository = invoiceRepository;
        _forecastService = forecastService;
    }
    public async Task<DashboardSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var openInvoices = await _invoiceRepository.CountByStatusAsync(InvoiceStatus.Open, cancellationToken);
        var closedInvoices = await _invoiceRepository.CountByStatusAsync(InvoiceStatus.Closed, cancellationToken);
        var processedItems = await _invoiceRepository.GetProcessedItemsCountAsync(cancellationToken);

        return new DashboardSummaryResponse(openInvoices, closedInvoices, processedItems);
    }
    public async Task<IReadOnlyCollection<DailyConsumptionResponse>> GetDailyConsumptionAsync(int days, CancellationToken cancellationToken = default)
    {
        days = Math.Clamp(days, 7, 90);
        var data = await _invoiceRepository.GetDailyConsumptionAsync(days, cancellationToken);

        return [.. data.Select(item => new DailyConsumptionResponse(item.Date, item.Quantity))];
    }
    public async Task<IReadOnlyCollection<TopProductResponse>> GetTopProductsAsync(int take, CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 10);
        var data = await _invoiceRepository.GetTopProductsAsync(take, cancellationToken);

        return [.. data.Select(item => new TopProductResponse(item.ProductId, item.Quantity))];
    }
    public async Task<ConsumptionForecastResponse> GetConsumptionForecastAsync(int historicalDays = 30, int forecastDays = 7, CancellationToken cancellationToken = default)
    {
        historicalDays = Math.Clamp(historicalDays, 14, 90);
        forecastDays = Math.Clamp(forecastDays, 1, 30);

        var consumption = await _invoiceRepository.GetDailyConsumptionAsync(historicalDays, cancellationToken);
        var today = DateTime.UtcNow.Date;
        var consumptionByDate = consumption.ToDictionary(item => item.Date.Date, item => item.Quantity);

        var history = Enumerable.Range(0, historicalDays).Select(index =>
        {
            var date = today.AddDays(-(historicalDays - 1 - index));
            return (float)
            consumptionByDate.GetValueOrDefault(date, 0);
        }).ToArray();

        var prediction = _forecastService.Predict(history, forecastDays);
        var average = history.Length == 0 ? 0 : history.Average();

        if (!prediction.HasEnoughData)
            return new ConsumptionForecastResponse(false, historicalDays, forecastDays, Math.Round(average, 2), 0, "Dados insuficientes", []);

        var forecast = prediction.Values.Select((value, index) => new ConsumptionForecastPointResponse(today.AddDays(index + 1), Math.Max(0, (int)Math.Round(value)))).ToList();
        var estimatedConsumption = forecast.Sum(item => item.EstimatedQuantity);
        var predictedAverage = forecast.Count == 0 ? 0 : forecast.Average(item => item.EstimatedQuantity);
        var trendPercentage = average <= 0 ? 0 : ((predictedAverage - average) / average) * 100;
        var trend = trendPercentage

            switch
        {
            > 5 => "Crescimento",
            < -5 => "Redução",
            _ => "Estável"
        };

        return new ConsumptionForecastResponse(true, historicalDays, forecastDays, Math.Round(average, 2), estimatedConsumption, trend, forecast);
    }
}
