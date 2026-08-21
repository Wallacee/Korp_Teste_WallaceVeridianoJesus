using Korp.Invoice.Inventory.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
namespace Korp.Invoice.Inventory.Api.Controllers;

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
}
