using Microsoft.EntityFrameworkCore;
using WorkOrderApi.Data;
using WorkOrderApi.Dtos;
using WorkOrderApi.Models;

namespace WorkOrderApi.Services;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync();
}

public class DashboardService(AppDbContext db) : IDashboardService
{
    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var workOrderStatusCounts = await db.WorkOrders
            .AsNoTracking()
            .GroupBy(w => w.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var openWorkOrders = workOrderStatusCounts
            .FirstOrDefault(x => x.Status == WorkOrderStatus.Open)?.Count ?? 0;
        var inProgressWorkOrders = workOrderStatusCounts
            .FirstOrDefault(x => x.Status == WorkOrderStatus.InProgress)?.Count ?? 0;
        var completedWorkOrders = workOrderStatusCounts
            .FirstOrDefault(x => x.Status == WorkOrderStatus.Done)?.Count ?? 0;
        var totalWorkOrders = openWorkOrders + inProgressWorkOrders + completedWorkOrders;

        var incidentStatusCounts = await db.IncidentReports
            .AsNoTracking()
            .GroupBy(i => i.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var totalIncidentReports = incidentStatusCounts.Sum(x => x.Count);
        var openIncidentReports = incidentStatusCounts
            .Where(x => x.Status != IncidentStatus.Resolved)
            .Sum(x => x.Count);

        var priorityGroups = await db.WorkOrders
            .AsNoTracking()
            .GroupBy(w => w.Priority)
            .Select(g => new { Priority = g.Key, Count = g.Count() })
            .ToListAsync();

        var workOrdersByPriority = new List<StatusCountDto>
        {
            new("High", priorityGroups.FirstOrDefault(x => x.Priority == Priority.High)?.Count ?? 0),
            new("Medium", priorityGroups.FirstOrDefault(x => x.Priority == Priority.Medium)?.Count ?? 0),
            new("Low", priorityGroups.FirstOrDefault(x => x.Priority == Priority.Low)?.Count ?? 0)
        };

        var severityGroups = await db.IncidentReports
            .AsNoTracking()
            .GroupBy(i => i.Severity)
            .Select(g => new { Severity = g.Key, Count = g.Count() })
            .ToListAsync();

        var incidentsBySeverity = new List<StatusCountDto>
        {
            new("Critical", severityGroups.FirstOrDefault(x => x.Severity == IncidentSeverity.Critical)?.Count ?? 0),
            new("High", severityGroups.FirstOrDefault(x => x.Severity == IncidentSeverity.High)?.Count ?? 0),
            new("Medium", severityGroups.FirstOrDefault(x => x.Severity == IncidentSeverity.Medium)?.Count ?? 0),
            new("Low", severityGroups.FirstOrDefault(x => x.Severity == IncidentSeverity.Low)?.Count ?? 0)
        };

        var branchBreakdown = await db.Branches
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .Select(b => new BranchBreakdownDto(
                b.Name,
                b.WorkOrders.Count,
                db.IncidentReports.Count(i => i.BranchId == b.Id)))
            .ToListAsync();

        var recentWorkOrders = await db.WorkOrders
            .AsNoTracking()
            .OrderByDescending(w => w.CreatedAt)
            .Take(5)
            .Select(w => new WorkOrderDto(
                w.Id,
                w.Title,
                w.Description,
                w.Status.ToString(),
                w.Priority.ToString(),
                w.BranchId,
                w.Branch.Name,
                w.CreatedAt))
            .ToListAsync();

        var recentIncidentReports = await db.IncidentReports
            .AsNoTracking()
            .OrderByDescending(i => i.CreatedAt)
            .Take(5)
            .Select(i => new IncidentReportDto(
                i.Id,
                i.Title,
                i.Description,
                i.ReportedBy,
                i.Status.ToString(),
                i.Severity.ToString(),
                i.BranchId,
                i.Branch.Name,
                i.CreatedAt))
            .ToListAsync();

        return new DashboardSummaryDto(
            totalWorkOrders,
            openWorkOrders,
            inProgressWorkOrders,
            completedWorkOrders,
            totalIncidentReports,
            openIncidentReports,
            workOrdersByPriority,
            incidentsBySeverity,
            branchBreakdown,
            recentWorkOrders,
            recentIncidentReports);
    }
}