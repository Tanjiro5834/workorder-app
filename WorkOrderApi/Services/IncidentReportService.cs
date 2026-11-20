using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WorkOrderApi.Data;
using WorkOrderApi.Dtos;
using WorkOrderApi.Models;

namespace WorkOrderApi.Services;

public interface IIncidentReportService
{
    Task<PagedResult<IncidentReportDto>> GetAllAsync(
        IncidentStatus? status,
        IncidentSeverity? severity,
        string? search,
        int page,
        int pageSize);
    Task<IncidentReportDto?> GetByIdAsync(int id);
    Task<IncidentReportDto?> CreateAsync(CreateIncidentReportRequest request);
    Task<(IncidentReportDto? IncidentReport, string? Error)> UpdateAsync(int id, UpdateIncidentReportRequest request);
    Task<bool> UpdateStatusAsync(int id, IncidentStatus status);
    Task<bool> DeleteAsync(int id);
}

public class IncidentReportService(AppDbContext db) : IIncidentReportService
{
    private static readonly Expression<Func<IncidentReport, IncidentReportDto>> ToDto = i =>
        new IncidentReportDto(
            i.Id,
            i.Title,
            i.Description,
            i.ReportedBy,
            i.Status.ToString(),
            i.Severity.ToString(),
            i.BranchId,
            i.Branch.Name,
            i.CreatedAt);

    public async Task<PagedResult<IncidentReportDto>> GetAllAsync(
        IncidentStatus? status,
        IncidentSeverity? severity,
        string? search,
        int page,
        int pageSize)
    {
        var query = db.IncidentReports.AsNoTracking();

        if (status is not null)
        {
            query = query.Where(i => i.Status == status);
        }

        if (severity is not null)
        {
            query = query.Where(i => i.Severity == severity);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(i =>
                i.Title.Contains(term) ||
                i.Branch.Name.Contains(term) ||
                (i.ReportedBy != null && i.ReportedBy.Contains(term)));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ToDto)
            .ToListAsync();

        return new PagedResult<IncidentReportDto>(items, total, page, pageSize);
    }

    public Task<IncidentReportDto?> GetByIdAsync(int id) =>
        db.IncidentReports
            .AsNoTracking()
            .Where(i => i.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync();

    public async Task<IncidentReportDto?> CreateAsync(CreateIncidentReportRequest request)
    {
        if (!await db.Branches.AnyAsync(b => b.Id == request.BranchId)) return null;

        var incident = new IncidentReport
        {
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),
            ReportedBy = string.IsNullOrWhiteSpace(request.ReportedBy)
                ? null
                : request.ReportedBy.Trim(),
            Severity = request.Severity,
            BranchId = request.BranchId
        };

        db.IncidentReports.Add(incident);
        await db.SaveChangesAsync();

        return await GetByIdAsync(incident.Id)
            ?? throw new Exception("Failed to retrieve the created incident report.");
    }

    public async Task<(IncidentReportDto? IncidentReport, string? Error)> UpdateAsync(
        int id,
        UpdateIncidentReportRequest request)
    {
        var incident = await db.IncidentReports.FindAsync(id);
        if (incident is null) return (null, null);

        if (!await db.Branches.AnyAsync(b => b.Id == request.BranchId))
            return (null, "Branch not found");

        incident.Title = request.Title.Trim();
        incident.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();
        incident.ReportedBy = string.IsNullOrWhiteSpace(request.ReportedBy)
            ? null
            : request.ReportedBy.Trim();
        incident.Severity = request.Severity;
        incident.BranchId = request.BranchId;

        await db.SaveChangesAsync();

        var updated = await GetByIdAsync(id)
            ?? throw new Exception("Failed to retrieve the updated incident report.");

        return (updated, null);
    }

    public async Task<bool> UpdateStatusAsync(int id, IncidentStatus status)
    {
        var incident = await db.IncidentReports.FindAsync(id);
        if (incident is null) return false;

        incident.Status = status;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id) =>
        await db.IncidentReports.Where(i => i.Id == id).ExecuteDeleteAsync() > 0;
}