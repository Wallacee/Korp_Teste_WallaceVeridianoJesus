using Korp.Invoice.Inventory.Application.Interfaces;
using Korp.Invoice.Inventory.Application.Requests;
using Microsoft.AspNetCore.Mvc;
namespace Korp.Invoice.Inventory.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly IProductAppService _productAppService;
    public ProductsController(IProductAppService productAppService)
    {
        _productAppService = productAppService;
    }
    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _productAppService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetByIdAsync), new { id = product.Id }, product);
    }
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _productAppService.GetByIdAsync(id, cancellationToken));
    }
    [HttpGet]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        return Ok(await _productAppService.GetAllAsync(cancellationToken));
    }

    [HttpPost("{id:guid}/stock/debit")]
    public async Task<IActionResult> DebitStockAsync(Guid id,DebitStockRequest request,CancellationToken cancellationToken)
    {
        await _productAppService.DebitStockAsync(id,request,cancellationToken);

        return NoContent();
    }
}
