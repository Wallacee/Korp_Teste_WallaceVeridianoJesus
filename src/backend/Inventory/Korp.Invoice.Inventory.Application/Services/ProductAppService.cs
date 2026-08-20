using FluentValidation;
using Korp.Invoice.Inventory.Application.DTOs;
using Korp.Invoice.Inventory.Application.Interfaces;
using Korp.Invoice.Inventory.Application.Requests;
using Korp.Invoice.Inventory.Domain.Entities;
using Korp.Invoice.Inventory.Domain.Exceptions;
using Korp.Invoice.Inventory.Domain.Interfaces;
using Korp.Invoice.Shared.Pagination;
namespace Korp.Invoice.Inventory.Application.Services;

public sealed class ProductAppService : IProductAppService
{
    private readonly IProductRepository _productRepository;
    private readonly IValidator<CreateProductRequest> _createProductValidator;
    private readonly IValidator<DebitStockRequest> _debitStockValidator;
    private readonly IStockOperationRepository _stockOperationRepository;
    private readonly IInventoryUnitOfWork _unitOfWork;
    private readonly IValidator<ProcessStockRequest> _processStockValidator;
    private readonly IValidator<UpdateProductRequest> _updateValidator;
    public ProductAppService(
    IProductRepository productRepository,
    IValidator<CreateProductRequest> createProductValidator,
    IValidator<DebitStockRequest> debitStockValidator,
    IStockOperationRepository stockOperationRepository,
    IInventoryUnitOfWork unitOfWork,
    IValidator<ProcessStockRequest> processStockValidator,
    IValidator<UpdateProductRequest> updateValidator)
    {
        _productRepository = productRepository;
        _createProductValidator = createProductValidator;
        _debitStockValidator = debitStockValidator;
        _stockOperationRepository = stockOperationRepository;
        _unitOfWork = unitOfWork;
        _processStockValidator = processStockValidator;
        _updateValidator = updateValidator;
    }
    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        await _createProductValidator.ValidateAndThrowAsync(request, cancellationToken);

        var existingProduct = await _productRepository.GetByCodeAsync(request.Code, cancellationToken);

        if (existingProduct is not null)
            throw new ConflictException($"Já existe um produto cadastrado com o código '{request.Code}'.");

        var product = new Product(request.Code, request.Description, request.Stock);

        await _productRepository.AddAsync(product, cancellationToken);

        return Map(product);
    }

    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var product = await _productRepository.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Produto", id);
        var productWithSameCode = await _productRepository.GetByCodeAsync(request.Code, cancellationToken);

        if (productWithSameCode is not null && productWithSameCode.Id != id)
            throw new ConflictException($"Já existe um produto com o código '{request.Code}'.");

        product.Update(request.Code, request.Description, request.Stock);

        await _productRepository.UpdateAsync(product, cancellationToken);

        return Map(product);
    }


    public async Task<ProductDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);

        return product is null ? throw new NotFoundException("Produto", id) : Map(product);
    }

    public async Task DebitStockAsync(Guid productId, DebitStockRequest request, CancellationToken cancellationToken = default)
    {
        await _debitStockValidator.ValidateAndThrowAsync(request, cancellationToken);
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken) ?? throw new NotFoundException("Produto", productId);
        product.DebitStock(request.Quantity);
        await _productRepository.UpdateAsync(product, cancellationToken);
    }

    public async Task ProcessStockAsync(ProcessStockRequest request, CancellationToken cancellationToken = default)
    {
        await _processStockValidator.ValidateAndThrowAsync(request, cancellationToken);
        var alreadyProcessed = await _stockOperationRepository.ExistsAsync(request.OperationId, cancellationToken);

        if (alreadyProcessed)
            return;

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {

            var productIds = request.Items.Select(x => x.ProductId).Distinct().ToList();
            var products = await _productRepository.GetByIdsAsync(productIds, ct);

            if (products.Count != productIds.Count)
            {
                var foundIds = products.Select(x => x.Id).ToHashSet();
                var missingId = productIds.First(x => !foundIds.Contains(x));
                throw new NotFoundException("Produto", missingId);
            }
            foreach (var item in request.Items)
            {
                var product = products.First(x => x.Id == item.ProductId);
                product.DebitStock(item.Quantity);
            }
            await _stockOperationRepository.AddAsync(new StockOperation(request.OperationId), ct);
        },
            cancellationToken);
    }

    public async Task<PagedResult<ProductDto>> SearchAsync(ProductSearchRequest request, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var (items, totalCount) = await _productRepository.SearchAsync(request.Search,page,pageSize,request.SortBy,request.SortDirection,cancellationToken);

        return new PagedResult<ProductDto>
        {
            Items = [.. items.Select(Map)],
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken) ??
            throw new NotFoundException("Produto", id);
        await _productRepository.DeleteAsync(product, cancellationToken);
    }

    public async Task<IReadOnlyCollection<ProductDto>> GetByIdsAsync(IEnumerable<Guid> ids,CancellationToken cancellationToken = default)
    {
        var productIds = ids.Where(id => id != Guid.Empty).Distinct().ToList();

        if (productIds.Count == 0)
            return [];

        var products = await _productRepository.GetByIdsAsync(productIds,cancellationToken);

        return [.. products.Select(Map)];
    }

    private static ProductDto Map(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Code = product.Code,
            Description = product.Description,
            Stock = product.Stock,
            CreatedAtUtc = product.CreatedAtUtc
        };
    }
}
