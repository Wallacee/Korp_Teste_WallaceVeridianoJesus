using System.Net.Http.Json;
using Korp.Invoice.Inventory.Application.ExternalServices;


namespace Korp.Invoice.Inventory.Infrastructure.ExternalServices.Billing;

public sealed class BillingHttpService : IBillingService
{
    private readonly HttpClient _httpClient;

    public BillingHttpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> IsProductInUseAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/invoices/products/{productId}/usage", cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ProductUsageResponse>(cancellationToken: cancellationToken);

        return result?.IsUsed ?? false;
    }
}
