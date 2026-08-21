using Korp.Invoice.Billing.Application.DTOs.Dashboard;
using Korp.Invoice.Billing.Application.Interfaces;
using Korp.Invoice.Billing.Domain.Enums;
using Korp.Invoice.Billing.Domain.Interfaces;
namespace Korp.Invoice.Billing.Application.Services;

public sealed class DashboardAppService : IDashboardAppService
{
    private readonly IInvoiceRepository _invoiceRepository;
    public DashboardAppService(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }
    public async Task<DashboardSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken =default)
    {
        var openInvoices = await _invoiceRepository.CountByStatusAsync(InvoiceStatus.Open, cancellationToken);
        var closedInvoices = await _invoiceRepository.CountByStatusAsync(InvoiceStatus.Closed, cancellationToken);
        var processedItems = await _invoiceRepository.GetProcessedItemsCountAsync(cancellationToken);

        return new DashboardSummaryResponse(openInvoices, closedInvoices, processedItems);
    }
    public async Task<IReadOnlyCollection<DailyConsumptionResponse>> GetDailyConsumptionAsync(int days, CancellationToken cancellationToken =default)
    {
        days = Math.Clamp(days, 7, 90);
        var data = await _invoiceRepository.GetDailyConsumptionAsync(days, cancellationToken);

        return [.. data.Select(item => new DailyConsumptionResponse(item.Date, item.Quantity))];
    }
    public async Task<IReadOnlyCollection<TopProductResponse>> GetTopProductsAsync(int take, CancellationToken cancellationToken =default)
    {
        take = Math.Clamp(take, 1, 10);
        var data = await _invoiceRepository.GetTopProductsAsync(take, cancellationToken);

        return [.. data.Select(item => new TopProductResponse(item.ProductId, item.Quantity))];
    }
}
