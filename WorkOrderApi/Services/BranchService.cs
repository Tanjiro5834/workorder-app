using Microsoft.EntityFrameworkCore;
using WorkOrderApi.Data;

namespace WorkOrderApi.Services;

public record BranchDto(int Id, string Name);

public interface IBranchService
{
    Task<List<BranchDto>> GetAllAsync();
}

public class BranchService(AppDbContext db) : IBranchService
{
    public Task<List<BranchDto>> GetAllAsync() =>
        db.Branches.AsNoTracking()
        .Select(b => new BranchDto(b.Id, b.Name))
        .ToListAsync();
}