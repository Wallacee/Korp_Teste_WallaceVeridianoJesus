using Korp.Invoice.Inventory.Application.DTOs;
using Korp.Invoice.Inventory.Application.Requests;

namespace Korp.Invoice.Inventory.Application.Interfaces;

public interface IProductAppService
{
    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task DebitStockAsync(Guid productId, DebitStockRequest request, CancellationToken cancellationToken = default);
    Task ProcessStockAsync(ProcessStockRequest request, CancellationToken cancellationToken = default);
}
