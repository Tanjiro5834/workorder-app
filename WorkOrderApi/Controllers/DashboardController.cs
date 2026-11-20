using Microsoft.AspNetCore.Mvc;
using WorkOrderApi.Dtos;
using WorkOrderApi.Services;

namespace WorkOrderApi.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController(IDashboardService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary() =>
        Ok(await service.GetSummaryAsync());
}