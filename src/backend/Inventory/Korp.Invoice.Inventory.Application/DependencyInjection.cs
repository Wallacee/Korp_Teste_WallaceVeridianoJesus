using FluentValidation;
using Korp.Invoice.Inventory.Application.Interfaces;
using Korp.Invoice.Inventory.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Korp.Invoice.Inventory.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining(typeof(DependencyInjection));

        services.AddScoped<IProductAppService, ProductAppService>();
        services.AddScoped<IDashboardAppService, DashboardAppService>();

        return services;
    }
}
