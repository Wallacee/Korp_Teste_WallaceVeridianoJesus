using FluentValidation;
using Korp.Invoice.Billing.Application.Exceptions;
using Korp.Invoice.Billing.Application.ExternalServices.Inventory;
using Korp.Invoice.Billing.Application.Requests;
using Korp.Invoice.Billing.Application.Services;
using Korp.Invoice.Billing.Domain.Entities;
using Korp.Invoice.Billing.Domain.Enums;
using Korp.Invoice.Billing.Domain.Exceptions;
using Korp.Invoice.Billing.Domain.Interfaces;
using Korp.Invoice.Billing.Domain.Services;
using Korp.Invoice.Inventory.Domain.Exceptions;
using Moq;

namespace Korp.Invoice.Billing.UnitTests.Application;

public sealed class InvoiceProcessingTests
{
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock = new();
    private readonly Mock<IInvoiceNumberGenerator> _invoiceNumberGeneratorMock = new();
    private readonly Mock<IInventoryService> _inventoryServiceMock = new();
    private readonly Mock<IValidator<CreateInvoiceRequest>> _createInvoiceValidatorMock = new();
    private readonly Mock<IValidator<UpdateInvoiceRequest>> _updateInvoiceValidatorMock = new();
    private readonly Mock<IBillingUnitOfWork> _unitOfWorkMock = new();
    [Fact]
    public async Task ProcessAsync_ShouldProcessStockAndCloseInvoice()
    {
        var productId = Guid.NewGuid();
        var invoice = new FiscalInvoice(1);

        invoice.AddItem(productId, 2);

        _invoiceRepositoryMock.Setup(x => x.GetByIdAsync(invoice.Id, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);
        _inventoryServiceMock.Setup(x => x.ProcessStockAsync(invoice.Id, It.IsAny<IReadOnlyCollection<InventoryStockItem>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var service = CreateService();
        var result = await service.ProcessAsync(invoice.Id);

        Assert.Equal(InvoiceStatus.Closed, result.Status);

        _inventoryServiceMock.Verify(x => x.ProcessStockAsync(invoice.Id, It.Is<IReadOnlyCollection<InventoryStockItem>>(items => items.Count == 1 && items.First().ProductId == productId && items.First().Quantity == 2), It.IsAny<CancellationToken>()), Times.Once);
        _invoiceRepositoryMock.Verify(x => x.UpdateAsync(invoice, It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task ProcessAsync_ShouldKeepInvoiceOpen_WhenInventoryFails()
    {
        var invoice = new FiscalInvoice(1);
        invoice.AddItem(Guid.NewGuid(), 2);

        _invoiceRepositoryMock.Setup(x => x.GetByIdAsync(invoice.Id, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);
        _inventoryServiceMock.Setup(x => x.ProcessStockAsync(invoice.Id, It.IsAny<IReadOnlyCollection<InventoryStockItem>>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InventoryUnavailableException());

        var service = CreateService();

        await Assert.ThrowsAsync<InventoryUnavailableException>(() => service.ProcessAsync(invoice.Id));

        Assert.Equal(InvoiceStatus.Open, invoice.Status);

        _invoiceRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<FiscalInvoice>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public async Task ProcessAsync_ShouldNotCallInventory_WhenInvoiceIsClosed()
    {
        var invoice = new FiscalInvoice(1);
        invoice.AddItem(Guid.NewGuid(), 2);
        invoice.Close();

        _invoiceRepositoryMock.Setup(x => x.GetByIdAsync(invoice.Id, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);

        var service = CreateService();
        await Assert.ThrowsAsync<InvoiceAlreadyClosedException>(() => service.ProcessAsync(invoice.Id));

        _inventoryServiceMock.Verify(x => x.ProcessStockAsync(It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<InventoryStockItem>>(), It.IsAny<CancellationToken>()), Times.Never);
        _invoiceRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<FiscalInvoice>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public async Task ProcessAsync_ShouldThrow_WhenInvoiceDoesNotExist()
    {
        var invoiceId = Guid.NewGuid();
        _invoiceRepositoryMock.Setup(x => x.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>())).ReturnsAsync((FiscalInvoice?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.ProcessAsync(invoiceId));
        _inventoryServiceMock.Verify(x => x.ProcessStockAsync(It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<InventoryStockItem>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_ShouldNotProcess_WhenInvoiceHasNoItems()
    {
        var invoice = new FiscalInvoice(1);

        _invoiceRepositoryMock.Setup(x => x.GetByIdAsync(invoice.Id, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);

        var service = CreateService();
        await Assert.ThrowsAsync<InvoiceWithoutItemsException>(() => service.ProcessAsync(invoice.Id));

        _inventoryServiceMock.Verify(x => x.ProcessStockAsync(It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<InventoryStockItem>>(), It.IsAny<CancellationToken>()), Times.Never);
        _invoiceRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<FiscalInvoice>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public async Task ProcessAsync_ShouldSendAllInvoiceItemsToInventory()
    {
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();
        var productId3 = Guid.NewGuid();
        var invoice = new FiscalInvoice(1);

        invoice.AddItem(productId1, 2);
        invoice.AddItem(productId2, 5);
        invoice.AddItem(productId3, 1);

        _invoiceRepositoryMock.Setup(x => x.GetByIdAsync(invoice.Id, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);
        _inventoryServiceMock.Setup(x => x.ProcessStockAsync(invoice.Id, It.IsAny<IReadOnlyCollection<InventoryStockItem>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var service = CreateService();
        await service.ProcessAsync(invoice.Id);

        _inventoryServiceMock.Verify(x => x.ProcessStockAsync(
            invoice.Id
            , It.Is<IReadOnlyCollection<InventoryStockItem>>(items => items.Count == 3
                && items.Any(x => x.ProductId == productId1 && x.Quantity == 2)
                && items.Any(x => x.ProductId == productId2 && x.Quantity == 5)
                && items.Any(x => x.ProductId == productId3 && x.Quantity == 1))
            , It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task ProcessAsync_ShouldPropagateCancellationToken()
    {
        var invoice = new FiscalInvoice(1);
        invoice.AddItem(Guid.NewGuid(), 2);

        using
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        _invoiceRepositoryMock.Setup(x => x.GetByIdAsync(invoice.Id, cancellationToken)).ReturnsAsync(invoice);
        _inventoryServiceMock.Setup(x => x.ProcessStockAsync(invoice.Id, It.IsAny<IReadOnlyCollection<InventoryStockItem>>(), cancellationToken)).Returns(Task.CompletedTask);

        var service = CreateService();
        await service.ProcessAsync(invoice.Id, cancellationToken);

        _invoiceRepositoryMock.Verify(x => x.GetByIdAsync(invoice.Id, cancellationToken), Times.Once);
        _inventoryServiceMock.Verify(x => x.ProcessStockAsync(invoice.Id, It.IsAny<IReadOnlyCollection<InventoryStockItem>>(), cancellationToken), Times.Once);
    }
    private InvoiceAppService CreateService()
    {
        return new InvoiceAppService(
            _invoiceRepositoryMock.Object
            , _invoiceNumberGeneratorMock.Object
            , _createInvoiceValidatorMock.Object
            , _updateInvoiceValidatorMock.Object
            , _inventoryServiceMock.Object
            , _unitOfWorkMock.Object);
    }
}
