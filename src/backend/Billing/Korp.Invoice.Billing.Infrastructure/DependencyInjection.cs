using Korp.Invoice.Billing.Application.ExternalServices.Inventory;
using Korp.Invoice.Billing.Domain.Repositories;
using Korp.Invoice.Billing.Domain.Services;
using Korp.Invoice.Billing.Infrastructure.ExternalServices.Inventory;
using Korp.Invoice.Billing.Infrastructure.Persistence;
using Korp.Invoice.Billing.Infrastructure.Repositories;
using Korp.Invoice.Billing.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Korp.Invoice.Billing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("BillingDatabase") ??
            throw new InvalidOperationException("A connection string 'BillingDatabase' não foi configurada.");

        var inventoryBaseUrl = configuration["Services:Inventory:BaseUrl"] ??
            throw new InvalidOperationException("A URL do serviço de estoque não foi configurada.");

        services.AddHttpClient<IInventoryService, InventoryHttpService>(client =>
        {
            client.BaseAddress = new Uri(inventoryBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        services.AddDbContext<BillingDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IInvoiceNumberGenerator, PostgresInvoiceNumberGenerator>();

        return services;
    }
}
