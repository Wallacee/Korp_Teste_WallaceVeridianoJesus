namespace Korp.Invoice.Inventory.Application.ExternalServices;

public interface IBillingService
{
    Task<bool> IsProductInUseAsync(Guid productId, CancellationToken cancellationToken = default);
}
