using System.ComponentModel.DataAnnotations;
using WorkOrderApi.Models;

namespace WorkOrderApi.Dtos;

public record WorkOrderDto(
    int Id,
    string Title,
    string? Description,
    string Status,
    string Priority,
    int BranchId,
    string BranchName,
    DateTime CreatedAt
);

public record CreateWorkOrderRequest(
    [Required, MaxLength(150)] string Title,
    [MaxLength(500)] string? Description,
    Priority Priority,
    [Range(1, int.MaxValue)] int BranchId
);

public record UpdateStatusRequest(WorkOrderStatus Status);

public record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);