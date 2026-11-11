using Microsoft.AspNetCore.Mvc;
using WorkOrderApi.Data;
using WorkOrderApi.Dtos;
using WorkOrderApi.Models;
using WorkOrderApi.Services;

namespace WorkOrderApi.Controllers;

[ApiController]
[Route("api/work-orders")]
public class WorkOrdersController(IWorkOrderService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<WorkOrderDto>>> GetAll(
         [FromQuery] WorkOrderStatus? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        return Ok(await service.GetAllAsync(status, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WorkOrderDto>> GetById(int id) =>
        await service.GetByIdAsync(id) is { } dto ? Ok(dto) : NotFound();

    [HttpPost]
    public async Task<ActionResult<WorkOrderDto>> Create(CreateWorkOrderRequest request)
    {
        var dto = await service.CreateAsync(request);
        if(dto is null) return BadRequest(new { error = "Branch not found"});
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult> UpdateStatus(int id, UpdateStatusRequest request) =>
        await service.UpdateStatusAsync(id, request.Status) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id) =>
        await service.DeleteAsync(id) ? NoContent() : NotFound();
}