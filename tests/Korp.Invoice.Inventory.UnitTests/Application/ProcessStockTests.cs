using FluentValidation;
using FluentValidation.Results;
using Korp.Invoice.Inventory.Application.Requests;
using Korp.Invoice.Inventory.Application.Services;
using Korp.Invoice.Inventory.Domain.Interfaces;
using Moq;
namespace Korp.Invoice.Inventory.UnitTests.Application;

public sealed class ProcessStockTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly Mock<IStockOperationRepository> _stockOperationRepositoryMock = new();
    private readonly Mock<IInventoryUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IValidator<ProcessStockRequest>> _validatorMock = new();
    private readonly Mock<IValidator<CreateProductRequest>> _createProductValidatorMock = new();
    private readonly Mock<IValidator<DebitStockRequest>> _debitStockValidatorMock = new();
    private readonly Mock<IValidator<ProcessStockRequest>> _processStockValidatorMock = new();
    private readonly Mock<IValidator<UpdateProductRequest>> _updateValidatorMock = new();


    [Fact]
    public async Task ProcessStockAsync_ShouldReturnWithoutChanges_WhenOperationWasAlreadyProcessed()
    {
        var request = new ProcessStockRequest
        {
            OperationId = Guid.NewGuid(),
            Items = [
                    new ProcessStockItemRequest {
                        ProductId = Guid.NewGuid(),
                            Quantity = 2
                    }
                ]
        };
        _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<ProcessStockRequest>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());
        _stockOperationRepositoryMock.Setup(x => x.ExistsAsync(request.OperationId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var service = CreateService();
        await service.ProcessStockAsync(request);
        _productRepositoryMock.Verify(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    private ProductAppService CreateService()
    {
        return new ProductAppService(
            _productRepositoryMock.Object,
            _createProductValidatorMock.Object,
            _debitStockValidatorMock.Object,
            _stockOperationRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _processStockValidatorMock.Object,
            _updateValidatorMock.Object);
    }
}
