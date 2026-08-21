using Korp.Invoice.Inventory.Application.DTOs.Dashboard;
using Korp.Invoice.Inventory.Application.Interfaces;
using Korp.Invoice.Inventory.Domain.Interfaces;
namespace Korp.Invoice.Inventory.Application.Services;

public sealed class DashboardAppService : IDashboardAppService
{
    private
    const int LowStockThreshold = 5;
    private readonly IProductRepository _productRepository;
    public DashboardAppService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }
    public async Task<InventorySummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var totalProducts = await _productRepository.CountAsync(cancellationToken);
        var totalStock = await _productRepository.GetTotalStockAsync(cancellationToken);
        var lowStockProducts = await _productRepository.CountLowStockAsync(LowStockThreshold, cancellationToken);

        return new InventorySummaryResponse(totalProducts, totalStock, lowStockProducts);
    }
}
