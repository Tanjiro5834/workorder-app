using Microsoft.EntityFrameworkCore;
using WorkOrderApi.Models;

namespace WorkOrderApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Branch>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.HasData(
                new Branch { Id = 1, Name = "Main Branch" },
                new Branch { Id = 2, Name = "Gentri Branch" });
        });

        b.Entity<WorkOrder>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(150).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Priority).HasConversion<string>().HasMaxLength(20);
        });
    }
}