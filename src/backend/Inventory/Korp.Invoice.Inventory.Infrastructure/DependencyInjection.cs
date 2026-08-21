using Korp.Invoice.Inventory.Application.ExternalServices;
using Korp.Invoice.Inventory.Domain.Interfaces;
using Korp.Invoice.Inventory.Infrastructure.ExternalServices.Billing;
using Korp.Invoice.Inventory.Infrastructure.Persistence;
using Korp.Invoice.Inventory.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Korp.Invoice.Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("InventoryDatabase")
            ?? throw new InvalidOperationException("A connection string 'InventoryDatabase' não foi configurada.");

        services.AddDbContext<InventoryDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IProductRepository, ProductRepository>();

        services.AddScoped<IStockOperationRepository, StockOperationRepository>();

        services.AddScoped<IInventoryUnitOfWork, InventoryUnitOfWork>();

        services.AddHttpClient<IBillingService, BillingHttpService>(client =>
        {
            client.BaseAddress = new Uri(
                configuration["Services:Billing"]!);
        });

        return services;
    }
}
