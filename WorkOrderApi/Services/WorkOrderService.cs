using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WorkOrderApi.Data;
using WorkOrderApi.Dtos;
using WorkOrderApi.Models;

namespace WorkOrderApi.Services;

public interface IWorkOrderService
{
    Task<PagedResult<WorkOrderDto>> GetAllAsync(WorkOrderStatus? status, int page, int pageSize);
    Task<WorkOrderDto?> GetByIdAsync(int id);
    Task<WorkOrderDto?> CreateAsync(CreateWorkOrderRequest req);
    Task<bool> UpdateStatusAsync(int id, WorkOrderStatus status);
    Task<bool> DeleteAsync(int id);
}

public class WorkOrderService(AppDbContext db) : IWorkOrderService
{
    private static readonly Expression<Func<WorkOrder, WorkOrderDto>> ToDto = w =>
        new WorkOrderDto(w.Id, w.Title, w.Description, w.Status.ToString(),
            w.Priority.ToString(), w.BranchId, w.Branch.Name, w.CreatedAt);
    
    public async Task<PagedResult<WorkOrderDto>> GetAllAsync(WorkOrderStatus? status, int page, int pageSize)
    {
        var query = db.WorkOrders.AsNoTracking();
        if(status is not null)
        {
            query = query.Where(w => w.Status == status);
        }

        var total = await query.CountAsync();
        var items = await query
        .OrderByDescending(w => w.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(ToDto)
        .ToListAsync();

        return new PagedResult<WorkOrderDto>(items, total, page, pageSize);
    }

    public Task<WorkOrderDto?> GetByIdAsync(int id) 
        => db.WorkOrders.AsNoTracking().Where(w => w.Id == id).Select(ToDto).FirstOrDefaultAsync();

    public async Task<WorkOrderDto?> CreateAsync(CreateWorkOrderRequest request)
    {
        if(!await db.Branches.AnyAsync(b => b.Id == request.BranchId)) return null;

        var workOrder = new WorkOrder
        {
            Title = request.Title.Trim(),
            Description = request.Description,
            Priority = request.Priority,
            BranchId = request.BranchId
        };

        db.WorkOrders.Add(workOrder);
        await db.SaveChangesAsync();
        return await GetByIdAsync(workOrder.Id ) ?? throw new Exception("Failed to retrieve the created work order.");
    }

    public async Task<bool> UpdateStatusAsync(int id, WorkOrderStatus status)
    {
        var workOrder = await db.WorkOrders.FindAsync(id);
        if(workOrder is null) return false;

        workOrder.Status = status;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id) => await db.WorkOrders.Where(w => w.Id == id).ExecuteDeleteAsync() > 0;
}