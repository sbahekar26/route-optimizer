using Solver;

var coordinates = new List<Coordinate>
{
    new Coordinate(43.3255, -79.7990),  // Burlington
    new Coordinate(43.6532, -79.3832),  // Toronto
    new Coordinate(43.5890, -79.6441),  // Oakville
    new Coordinate(43.4675, -79.6877),  // Bronte
};

var matrix = DistanceCalculator.BuildMatrix(coordinates);

var solver = new RouteSolver();
var result = solver.Solve(matrix);

Console.WriteLine($"Route: {string.Join(" -> ", result.Route)}");
Console.WriteLine($"Total cost (meters): {result.TotalCost}");