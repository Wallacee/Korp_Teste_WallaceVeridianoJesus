using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Korp.Invoice.Inventory.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining(typeof(DependencyInjection));

        return services;
    }
}
