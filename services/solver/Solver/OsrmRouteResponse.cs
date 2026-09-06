namespace Solver;

public record OsrmRouteResponse(string Code, OsrmRoute[] Routes);
public record OsrmRoute(OsrmGeometry Geometry);
public record OsrmGeometry(double[][] Coordinates);
