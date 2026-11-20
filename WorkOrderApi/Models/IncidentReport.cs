namespace WorkOrderApi.Models;

public enum IncidentStatus
{
    Open,
    Investigating,
    Resolved
}

public enum IncidentSeverity
{
    Low,
    Medium,
    High,
    Critical
}

public class IncidentReport
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string? ReportedBy { get; set; }

    public IncidentStatus Status { get; set; } = IncidentStatus.Open;
    public IncidentSeverity Severity { get; set; } = IncidentSeverity.Medium;
    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}