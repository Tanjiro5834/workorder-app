using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using WorkOrderApi.Data;
using WorkOrderApi.Models;
using WorkOrderApi.Dtos;

namespace WorkOrderApi.Services;

public interface IBranchService
{
    Task<List<BranchDto>> GetAllAsync();
    Task<(BranchDto? Branch, string? Error)> CreateAsync(CreateBranchRequest request);
}

public class BranchService(AppDbContext db) : IBranchService
{
    public Task<List<BranchDto>> GetAllAsync() =>
        db.Branches
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .Select(b => new BranchDto(
                b.Id,
                b.Name,
                b.Address,
                b.WorkOrders.Count,
                db.IncidentReports.Count(i => i.BranchId == b.Id)))
            .ToListAsync();

    public async Task<(BranchDto? Branch, string? Error)> CreateAsync(CreateBranchRequest request)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return (null, "Branch name is required.");

        var exists = await db.Branches.AnyAsync(b => b.Name.ToLower() == name.ToLower());
        if (exists)
            return (null, "A branch with this name already exists.");

        var branch = new Branch
        {
            Name = name,
            Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim()
        };

        db.Branches.Add(branch);
        await db.SaveChangesAsync();

        return (new BranchDto(branch.Id, branch.Name, branch.Address, 0, 0), null);
    }
}