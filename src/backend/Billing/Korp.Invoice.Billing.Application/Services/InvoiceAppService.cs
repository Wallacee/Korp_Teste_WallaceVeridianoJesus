using FluentValidation;
using Korp.Invoice.Billing.Application.DTOs;
using Korp.Invoice.Billing.Application.ExternalServices.Inventory;
using Korp.Invoice.Billing.Application.Interfaces;
using Korp.Invoice.Billing.Application.Requests;
using Korp.Invoice.Billing.Domain.Entities;
using Korp.Invoice.Billing.Domain.Repositories;
using Korp.Invoice.Billing.Domain.Services;
using Korp.Invoice.Inventory.Domain.Exceptions;

namespace Korp.Invoice.Billing.Application.Services;

public sealed class InvoiceAppService : IInvoiceAppService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IInvoiceNumberGenerator _invoiceNumberGenerator;
    private readonly IInventoryService _inventoryService;
    private readonly IValidator<CreateInvoiceRequest> _createInvoiceValidator;
    public InvoiceAppService(
     IInvoiceRepository invoiceRepository,
     IInvoiceNumberGenerator invoiceNumberGenerator,
     IValidator<CreateInvoiceRequest> createInvoiceValidator,
     IInventoryService inventoryService)
    {
        _invoiceRepository = invoiceRepository;
        _invoiceNumberGenerator = invoiceNumberGenerator;
        _createInvoiceValidator = createInvoiceValidator;
        _inventoryService = inventoryService;
    }

    public async Task<FiscalInvoiceDto> CreateAsync(CreateInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        await _createInvoiceValidator.ValidateAndThrowAsync(request, cancellationToken);

        foreach (var item in request.Items)
        {
            var product = await _inventoryService.GetProductByIdAsync(item.ProductId, cancellationToken);

            if (product is null)
                throw new NotFoundException("Produto", item.ProductId);
        }

        var number = await _invoiceNumberGenerator.GetNextAsync(cancellationToken);

        var invoice = new FiscalInvoice(number);

        foreach (var item in request.Items)
            invoice.AddItem(item.ProductId, item.Quantity);

        await _invoiceRepository.AddAsync(invoice, cancellationToken);

        return Map(invoice);
    }

    public async Task<FiscalInvoiceDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, cancellationToken);

        return invoice is null ? throw new NotFoundException("Nota fiscal", id) : Map(invoice);
    }

    public async Task<IReadOnlyCollection<FiscalInvoiceDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var invoices = await _invoiceRepository.GetAllAsync(cancellationToken);

        return [.. invoices.Select(Map)];
    }

    public async Task<FiscalInvoiceDto> ProcessAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Nota fiscal", id);

        invoice.EnsureCanBeProcessed();

        foreach (var item in invoice.Items)
            await _inventoryService.DebitStockAsync(item.ProductId, item.Quantity, cancellationToken);

        invoice.Close();

        await _invoiceRepository.UpdateAsync(invoice, cancellationToken);

        return Map(invoice);
    }

    private static FiscalInvoiceDto Map(FiscalInvoice invoice)
    {
        return new FiscalInvoiceDto
        {
            Id = invoice.Id,
            Number = invoice.Number,
            Status = invoice.Status,
            Items = [.. invoice.Items
                .Select(item => new InvoiceItemDto
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                })]
        };
    }
}
