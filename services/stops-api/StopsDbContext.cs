using Microsoft.EntityFrameworkCore;

namespace StopsApi;

public class StopsDbContext : DbContext
{
    public StopsDbContext(DbContextOptions<StopsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Stop> Stops => Set<Stop>();
}