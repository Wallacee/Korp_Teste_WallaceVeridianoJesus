using Korp.Invoice.Inventory.Application.DTOs;
using Korp.Invoice.Inventory.Application.Requests;
using Korp.Invoice.Shared.Pagination;

namespace Korp.Invoice.Inventory.Application.Interfaces;

public interface IProductAppService
{
    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ProductDto>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task DebitStockAsync(Guid productId, DebitStockRequest request, CancellationToken cancellationToken = default);
    Task ProcessStockAsync(ProcessStockRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductDto>> SearchAsync(ProductSearchRequest request, CancellationToken cancellationToken = default);
    Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
