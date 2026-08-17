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
}
