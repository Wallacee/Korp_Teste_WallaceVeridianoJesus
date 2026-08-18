using FluentValidation;
using Korp.Invoice.Inventory.Application.DTOs;
using Korp.Invoice.Inventory.Application.Interfaces;
using Korp.Invoice.Inventory.Application.Requests;
using Korp.Invoice.Inventory.Domain.Entities;
using Korp.Invoice.Inventory.Domain.Exceptions;
using Korp.Invoice.Inventory.Domain.Repositories;
namespace Korp.Invoice.Inventory.Application.Services;

public sealed class ProductAppService : IProductAppService
{
    private readonly IProductRepository _productRepository;
    private readonly IValidator<CreateProductRequest> _createProductValidator;
    private readonly IValidator<DebitStockRequest> _debitStockValidator;
    public ProductAppService(
    IProductRepository productRepository,
    IValidator<CreateProductRequest> createProductValidator,
    IValidator<DebitStockRequest> debitStockValidator)
    {
        _productRepository = productRepository;
        _createProductValidator = createProductValidator;
        _debitStockValidator = debitStockValidator;
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
    public async Task<ProductDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);

        return product is null ? throw new NotFoundException("Produto", id) : Map(product);
    }
    public async Task<IReadOnlyCollection<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetAllAsync(cancellationToken);

        return [.. products.Select(Map)];
    }

    public async Task DebitStockAsync(Guid productId, DebitStockRequest request, CancellationToken cancellationToken = default)
    {
        await _debitStockValidator.ValidateAndThrowAsync(request, cancellationToken);
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken) ?? throw new NotFoundException("Produto", productId);
        product.DebitStock(request.Quantity);
        await _productRepository.UpdateAsync(product, cancellationToken);
    }
    private static ProductDto Map(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Code = product.Code,
            Description = product.Description,
            Stock = product.Stock
        };
    }
}
