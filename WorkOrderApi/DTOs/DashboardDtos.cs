namespace WorkOrderApi.Dtos;

public record StatusCountDto(string Label, int Count);

public record BranchBreakdownDto(string BranchName, int WorkOrders, int Incidents);

public record DashboardSummaryDto(
    int TotalWorkOrders,
    int OpenWorkOrders,
    int InProgressWorkOrders,
    int CompletedWorkOrders,
    int TotalIncidentReports,
    int OpenIncidentReports,
    IReadOnlyList<StatusCountDto> WorkOrdersByPriority,
    IReadOnlyList<StatusCountDto> IncidentsBySeverity,
    IReadOnlyList<BranchBreakdownDto> BranchBreakdown,
    IReadOnlyList<WorkOrderDto> RecentWorkOrders,
    IReadOnlyList<IncidentReportDto> RecentIncidentReports
);