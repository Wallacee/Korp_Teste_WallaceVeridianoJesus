using Korp.Invoice.Billing.Application.Interfaces;
using Korp.Invoice.Billing.Application.Requests;
using Microsoft.AspNetCore.Mvc;
namespace Korp.Invoice.Billing.Api.Controllers;

[ApiController]
[Route("api/invoices")]
public sealed class FiscalInvoicesController : ControllerBase
{
    private readonly IInvoiceAppService _invoiceAppService;
    public FiscalInvoicesController(IInvoiceAppService invoiceAppService)
    {
        _invoiceAppService = invoiceAppService;
    }

    [HttpGet]
    public async Task<IActionResult> SearchAsync([FromQuery] InvoiceSearchRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _invoiceAppService.SearchAsync(request, cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateInvoiceRequest request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceAppService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetByIdAsync), new { id = invoice.Id }, invoice);
    }
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _invoiceAppService.GetByIdAsync(id, cancellationToken));
    }
    
    [HttpPost("{id:guid}/process")]
    public async Task<IActionResult> ProcessAsync(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _invoiceAppService.ProcessAsync(id, cancellationToken));
    }
}
