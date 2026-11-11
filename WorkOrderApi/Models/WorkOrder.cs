namespace WorkOrderApi.Models;

public enum WorkOrderStatus
{
    Open,
    InProgress,
    Done
}

public enum Priority
{
    Low,
    Medium,
    High
}

public class Branch
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Address { get; set; }

    public List<WorkOrder> WorkOrders { get; set; } = [];
}

public class WorkOrder
{
    public int Id {get ; set;}
    public string Title{get; set;} = "";
    public string? Description {get; set;} = "";

    public WorkOrderStatus Status {get; set;} = WorkOrderStatus.Open;
    public Priority Priority {get; set;} = Priority.Medium;
    public int BranchId {get; set;}
    public Branch Branch {get; set;} = null!;
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;

}