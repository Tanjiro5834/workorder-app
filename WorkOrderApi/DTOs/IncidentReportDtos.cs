using System.ComponentModel.DataAnnotations;
using WorkOrderApi.Models;

namespace WorkOrderApi.Dtos;

public record IncidentReportDto(
    int Id,
    string Title,
    string? Description,
    string? ReportedBy,
    string Status,
    string Severity,
    int BranchId,
    string BranchName,
    DateTime CreatedAt
);

public record CreateIncidentReportRequest(
    [Required, MaxLength(150)] string Title,
    [MaxLength(500)] string? Description,
    [MaxLength(100)] string? ReportedBy,
    IncidentSeverity Severity,
    [Range(1, int.MaxValue)] int BranchId
);

public record UpdateIncidentReportRequest(
    [Required, MaxLength(150)] string Title,
    [MaxLength(500)] string? Description,
    [MaxLength(100)] string? ReportedBy,
    IncidentSeverity Severity,
    [Range(1, int.MaxValue)] int BranchId
);

public record UpdateIncidentStatusRequest(IncidentStatus Status);