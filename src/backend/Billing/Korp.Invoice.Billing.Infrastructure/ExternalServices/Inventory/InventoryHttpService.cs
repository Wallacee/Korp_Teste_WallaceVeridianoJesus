using System.Net;
using System.Net.Http.Json;
using Korp.Invoice.Billing.Application.Exceptions;
using Korp.Invoice.Billing.Application.ExternalServices.Inventory;
using Korp.Invoice.Billing.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Polly.Timeout;


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
        try
        {
            var response = await _httpClient.GetAsync($"api/products/{productId}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<InventoryProductDto>(cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            throw new InventoryUnavailableException(exception);
        }
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
            Items = [..items.Select(item => new ProcessStockItemRequest {
                ProductId = item.ProductId,
                    Quantity = item.Quantity
            })]
        };
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsJsonAsync("api/products/stock/process", request, cancellationToken);
        }
        catch (TimeoutRejectedException exception)
        {
            throw new InventoryUnavailableException(exception);
        }
        catch (HttpRequestException exception)
        {
            throw new InventoryUnavailableException(exception);
        }

        if (response.IsSuccessStatusCode)
            return;

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: cancellationToken);

        if (response.StatusCode == HttpStatusCode.Conflict)
            throw new InventoryBusinessException(problem?.Detail ?? "Não foi possível processar o estoque.");

        throw new InventoryUnavailableException();
    }


}
