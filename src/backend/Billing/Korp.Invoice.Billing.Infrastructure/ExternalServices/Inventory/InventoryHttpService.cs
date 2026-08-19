using System.Net;
using System.Net.Http.Json;
using Korp.Invoice.Billing.Application.ExternalServices.Inventory;


namespace Korp.Invoice.Billing.Infrastructure.ExternalServices.Inventory;

public sealed class InventoryHttpService : IInventoryService
{
    private readonly HttpClient _httpClient;

    public InventoryHttpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<InventoryProductDto?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/products/{productId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<InventoryProductDto>(cancellationToken);
    }

    public async Task DebitStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        var request = new DebitStockRequest
        {
            Quantity = quantity
        };

        var response = await _httpClient.PostAsJsonAsync($"api/products/{productId}/stock/debit", request, cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task ProcessStockAsync(Guid operationId, IReadOnlyCollection<InventoryStockItem> items, CancellationToken cancellationToken = default)
    {
        var request = new ProcessStockRequest
        {
            OperationId = operationId,

            Items = [.. items
                .Select(x => new ProcessStockItemRequest
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity
                })]
        };

        var response = await _httpClient.PostAsJsonAsync("api/products/stock/process", request, cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
