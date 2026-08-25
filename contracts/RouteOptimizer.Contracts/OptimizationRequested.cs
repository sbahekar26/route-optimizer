namespace RouteOptimizer.Contracts;

public record OptimizationRequested(Guid JobId, List<Coordinate> Stops);
