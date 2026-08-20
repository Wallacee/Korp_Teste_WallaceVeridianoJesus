using FluentValidation;
using Korp.Invoice.Billing.Application.DTOs;
using Korp.Invoice.Billing.Application.ExternalServices.Inventory;
using Korp.Invoice.Billing.Application.Interfaces;
using Korp.Invoice.Billing.Application.Requests;
using Korp.Invoice.Billing.Domain.Entities;
using Korp.Invoice.Billing.Domain.Interfaces;
using Korp.Invoice.Billing.Domain.Services;
using Korp.Invoice.Inventory.Domain.Exceptions;
using Korp.Invoice.Shared.Pagination;

namespace Korp.Invoice.Billing.Application.Services;

public sealed class InvoiceAppService : IInvoiceAppService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IInvoiceNumberGenerator _invoiceNumberGenerator;
    private readonly IInventoryService _inventoryService;
    private readonly IValidator<CreateInvoiceRequest> _createInvoiceValidator;
    private readonly IValidator<UpdateInvoiceRequest> _updateInvoiceValidator;
    private readonly IBillingUnitOfWork _unitOfWork;
    public InvoiceAppService(
     IInvoiceRepository invoiceRepository,
     IInvoiceNumberGenerator invoiceNumberGenerator,
     IValidator<CreateInvoiceRequest> createInvoiceValidator,
     IValidator<UpdateInvoiceRequest> updateInvoiceValidator,
     IInventoryService inventoryService,
     IBillingUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _invoiceNumberGenerator = invoiceNumberGenerator;
        _createInvoiceValidator = createInvoiceValidator;
        _updateInvoiceValidator = updateInvoiceValidator;
        _inventoryService = inventoryService;
        _unitOfWork = unitOfWork;
    }
    public async Task<PagedResult<FiscalInvoiceDto>> SearchAsync(InvoiceSearchRequest request, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var (items, totalCount) = await _invoiceRepository.SearchAsync(request.Search, page, pageSize, request.SortBy ?? "number", request.SortDirection, cancellationToken);
        return new PagedResult<FiscalInvoiceDto>
        {
            Items = [.. items.Select(Map)],
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
    public async Task<FiscalInvoiceDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, cancellationToken);

        return invoice is null ? throw new NotFoundException("Nota fiscal", id) : Map(invoice);
    }
    public async Task<FiscalInvoiceDto> CreateAsync(CreateInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        await _createInvoiceValidator.ValidateAndThrowAsync(request, cancellationToken);
        var number = await _invoiceNumberGenerator.GetNextAsync(cancellationToken);

        var invoice = new FiscalInvoice(number);

        foreach (var item in request.Items)
            invoice.AddItem(item.ProductId, item.Quantity);

        await _invoiceRepository.AddAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(invoice);
    }
    public async Task<FiscalInvoiceDto> UpdateAsync(Guid id, UpdateInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        await _updateInvoiceValidator.ValidateAndThrowAsync(request, cancellationToken);

        var invoice = await _invoiceRepository.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Nota fiscal", id);

        invoice.EnsureOpen();

        invoice.ReplaceItems(request.Items.Select(x => (x.ProductId, x.Quantity)));

        await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(invoice);
    }
    public async Task<FiscalInvoiceDto> ProcessAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Nota fiscal", id);
        invoice.EnsureCanBeProcessed();

        var stockItems = invoice.Items.Select(item => new InventoryStockItem(item.ProductId, item.Quantity)).ToList();

        await _inventoryService.ProcessStockAsync(invoice.Id, stockItems, cancellationToken);
        invoice.Close();

        await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(invoice);
    }
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Nota fiscal", id);

        invoice.EnsureOpen();

        await _invoiceRepository.DeleteAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
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
