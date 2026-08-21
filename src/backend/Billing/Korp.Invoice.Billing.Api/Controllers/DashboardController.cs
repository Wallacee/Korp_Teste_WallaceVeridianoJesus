using Korp.Invoice.Billing.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
namespace Korp.Invoice.Billing.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardAppService _dashboardAppService;
    public DashboardController(IDashboardAppService dashboardAppService)
    {
        _dashboardAppService = dashboardAppService;
    }
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var result = await _dashboardAppService.GetSummaryAsync(cancellationToken);
        return Ok(result);
    }
    [HttpGet("consumption")]
    public async Task<IActionResult> GetDailyConsumptionAsync([FromQuery] int days = 30, CancellationToken cancellationToken = default)
    {
        var result = await _dashboardAppService.GetDailyConsumptionAsync(days, cancellationToken);
        return Ok(result);
    }
    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopProductsAsync([FromQuery] int take = 5, CancellationToken cancellationToken = default)
    {
        var result = await _dashboardAppService.GetTopProductsAsync(take, cancellationToken);
        return Ok(result);
    }
}
