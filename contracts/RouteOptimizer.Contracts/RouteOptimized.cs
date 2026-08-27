namespace RouteOptimizer.Contracts;

public record RouteOptimized(Guid JobId, long[] Route, long TotalCost);
