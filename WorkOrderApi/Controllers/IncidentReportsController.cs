using Microsoft.AspNetCore.Mvc;
using WorkOrderApi.Dtos;
using WorkOrderApi.Models;
using WorkOrderApi.Services;

namespace WorkOrderApi.Controllers;

[ApiController]
[Route("api/incident-reports")]
public class IncidentReportsController(IIncidentReportService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<IncidentReportDto>>> GetAll(
        [FromQuery] IncidentStatus? status,
        [FromQuery] IncidentSeverity? severity,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        return Ok(await service.GetAllAsync(status, severity, search, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<IncidentReportDto>> GetById(int id) =>
        await service.GetByIdAsync(id) is { } dto ? Ok(dto) : NotFound();

    [HttpPost]
    public async Task<ActionResult<IncidentReportDto>> Create(CreateIncidentReportRequest request)
    {
        var dto = await service.CreateAsync(request);
        if (dto is null) return BadRequest(new { error = "Branch not found" });
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<IncidentReportDto>> Update(int id, UpdateIncidentReportRequest request)
    {
        var (dto, error) = await service.UpdateAsync(id, request);
        if (error is not null) return BadRequest(new { error });
        if (dto is null) return NotFound();
        return Ok(dto);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult> UpdateStatus(int id, UpdateIncidentStatusRequest request) =>
        await service.UpdateStatusAsync(id, request.Status) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id) =>
        await service.DeleteAsync(id) ? NoContent() : NotFound();
}