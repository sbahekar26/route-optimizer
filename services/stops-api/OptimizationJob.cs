using System.ComponentModel.DataAnnotations;

namespace StopsApi;

public class OptimizationJob
{
    [Key]
    public Guid JobId { get; set; }
    public long[] Route { get; set; } = [];
    public long TotalCost { get; set; }
    public DateTime CompletedAt { get; set; }
}
