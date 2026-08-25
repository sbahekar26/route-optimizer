namespace Solver;

public record OsrmTableResponse(
    string Code,
    double[][] Durations,
    double[][] Distances
);