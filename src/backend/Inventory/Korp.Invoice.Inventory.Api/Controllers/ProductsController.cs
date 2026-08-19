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
    public async Task<IActionResult> CreateAsync([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _productAppService.CreateAsync(request, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, product);
    }
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _productAppService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost("{id:guid}/stock/debit")]
    public async Task<IActionResult> DebitStockAsync(Guid id, DebitStockRequest request, CancellationToken cancellationToken)
    {
        await _productAppService.DebitStockAsync(id, request, cancellationToken);

        return NoContent();
    }

    [HttpPost("stock/process")]
    public async Task<IActionResult> ProcessStockAsync(ProcessStockRequest request, CancellationToken cancellationToken)
    {
        await _productAppService.ProcessStockAsync(request, cancellationToken);

        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> SearchAsync([FromQuery] ProductSearchRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _productAppService.SearchAsync(request, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _productAppService.UpdateAsync(id, request, cancellationToken);
        return Ok(product);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await _productAppService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}
