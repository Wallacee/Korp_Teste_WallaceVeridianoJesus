using Korp.Invoice.Inventory.Domain.Entities;

namespace Korp.Invoice.Inventory.Domain.Repositories;

public interface IStockOperationRepository
{
    Task<bool> ExistsAsync(Guid operationId, CancellationToken cancellationToken = default);

    Task AddAsync(StockOperation operation, CancellationToken cancellationToken = default);
}
