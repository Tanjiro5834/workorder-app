namespace WorkOrderApi.Dtos;
using System.ComponentModel.DataAnnotations;

public record BranchDto(int Id, string Name, string? Address, int WorkOrderCount, int IncidentCount);

public record CreateBranchRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(200)] string? Address
);