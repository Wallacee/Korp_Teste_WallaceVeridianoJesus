using FluentValidation;
using FluentValidation.Results;
using Korp.Invoice.Inventory.Application.Requests;
using Korp.Invoice.Inventory.Application.Services;
using Korp.Invoice.Inventory.Domain.Entities;
using Korp.Invoice.Inventory.Domain.Exceptions;
using Korp.Invoice.Inventory.Domain.Interfaces;
using Moq;
namespace Korp.Invoice.Inventory.UnitTests.Application;

public sealed class ProductAppServiceTests
{
    private readonly Mock<IProductRepository> _repositoryMock = new();
    private readonly Mock<IValidator<CreateProductRequest>> _validatorMock = new();
    private readonly Mock<IValidator<DebitStockRequest>> _debitStockValidatorMock = new();
    private readonly Mock<IStockOperationRepository> _stockOperationRepositoryMock = new();
    private readonly Mock<IInventoryUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IValidator<ProcessStockRequest>> _processStockValidatorMock = new();

    private ProductAppService CreateService()
    {
        return new ProductAppService(_repositoryMock.Object
            , _validatorMock.Object
            , _debitStockValidatorMock.Object
            , _stockOperationRepositoryMock.Object
            , _unitOfWorkMock.Object
            , _processStockValidatorMock.Object);
    }
    [Fact]
    public async Task CreateAsync_ShouldCreateProduct_WhenRequestIsValid()
    {
        var request = new CreateProductRequest { Code = "PROD-001", Description = "Teclado mecânico", Stock = 10 };
        _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<CreateProductRequest>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _repositoryMock.Setup(x => x.GetByCodeAsync(request.Code, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);
        var service = CreateService();
        var result = await service.CreateAsync(request);
        Assert.Equal(request.Code, result.Code);
        Assert.Equal(request.Description, result.Description);
        Assert.Equal(request.Stock, result.Stock);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenCodeAlreadyExists()
    {
        var request = new CreateProductRequest { Code = "PROD-001", Description = "Teclado mecânico", Stock = 10 };
        _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<CreateProductRequest>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _repositoryMock.Setup(x => x.GetByCodeAsync(request.Code, It.IsAny<CancellationToken>())).ReturnsAsync(new Product(request.Code, request.Description, request.Stock));
        var service = CreateService();
        await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrow_WhenProductDoesNotExist()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);
        var service = CreateService();
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(id));
    }

    [Fact]
    public async Task DebitStockAsync_ShouldUpdateProductStock_WhenRequestIsValid()
    {
        var id = Guid.NewGuid();
        var request = new DebitStockRequest { Quantity = 5 };
        _debitStockValidatorMock.Setup(x => x.ValidateAsync(
            It.IsAny<ValidationContext<DebitStockRequest>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _repositoryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(new Product("PROD-001", "Teclado mecânico", 10));
        var service = CreateService();
        await service.DebitStockAsync(id, request);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DebitStockAsync_ShouldThrow_WhenRequestIsInvalid()
    {
        var id = Guid.NewGuid();
        var request = new DebitStockRequest
        {
            Quantity = 0
        };
        _debitStockValidatorMock
    .Setup(x => x.ValidateAsync(
        It.IsAny<IValidationContext>(),
        It.IsAny<CancellationToken>()))
    .ReturnsAsync(new ValidationResult(
    [
        new ValidationFailure(nameof(DebitStockRequest.Quantity),"Quantidade deve ser maior que zero.")
    ]));

        var service = CreateService(); await Assert.ThrowsAsync<NotFoundException>(() => service.DebitStockAsync(id, request));
    }
}
