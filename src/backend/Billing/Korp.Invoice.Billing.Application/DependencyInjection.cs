using Korp.Invoice.Billing.Application.Interfaces;
using Korp.Invoice.Billing.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Korp.Invoice.Billing.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IInvoiceAppService, InvoiceAppService>();

        return services;
    }
}
