using Microsoft.AspNetCore.Mvc;
using WorkOrderApi.Services;

namespace WorkOrderApi.Controllers;

[ApiController]
[Route("api/branches")]
public class BranchesController(IBranchService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<BranchDto>>> GetAll() => Ok(await service.GetAllAsync());
}