using Microsoft.EntityFrameworkCore;

namespace StopsApi;

public class StopsDbContext : DbContext
{
    public StopsDbContext(DbContextOptions<StopsDbContext> options)
        : base(options)
    {
    }
    public DbSet<OptimizationJob> OptimizationJobs => Set<OptimizationJob>();
    public DbSet<Stop> Stops => Set<Stop>();
}