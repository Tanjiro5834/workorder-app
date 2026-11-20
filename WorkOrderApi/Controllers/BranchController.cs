using Microsoft.AspNetCore.Mvc;
using WorkOrderApi.Services;
using WorkOrderApi.Dtos;

namespace WorkOrderApi.Controllers;

[ApiController]
[Route("api/branches")]
public class BranchesController(IBranchService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<BranchDto>>> GetAll() => Ok(await service.GetAllAsync());

    [HttpPost]
    public async Task<ActionResult<BranchDto>> Create(CreateBranchRequest request)
    {
        var (dto, error) = await service.CreateAsync(request);
        if (error is not null) return BadRequest(new { error });
        return Created($"/api/branches/{dto!.Id}", dto);
    }
}