using Google.OrTools.ConstraintSolver;

long[,] distanceMatrix =
{
    { 0, 10, 15, 20 },
    { 10, 0, 35, 25 },
    { 15, 35, 0, 30 },
    { 20, 25, 30, 0 },
};

int vehicleCount = 1;
int depot = 0;

RoutingIndexManager manager = new RoutingIndexManager(
    distanceMatrix.GetLength(0), vehicleCount, depot);

RoutingModel routing = new RoutingModel(manager);

int transitCallbackIndex = routing.RegisterTransitCallback((long fromIndex, long toIndex) =>
{
    var fromNode = manager.IndexToNode(fromIndex);
    var toNode = manager.IndexToNode(toIndex);
    var cost = distanceMatrix[fromNode, toNode];
    Console.WriteLine($"callback: {fromNode}->{toNode} = {cost}");
    return cost;
});

routing.SetArcCostEvaluatorOfAllVehicles(transitCallbackIndex);

RoutingSearchParameters searchParameters =
    operations_research_constraint_solver.DefaultRoutingSearchParameters();
searchParameters.FirstSolutionStrategy =
    FirstSolutionStrategy.Types.Value.PathCheapestArc;

Assignment solution = routing.SolveWithParameters(searchParameters);

var index = routing.Start(0);
Console.Write("Route: ");
long routeCost = 0;
while (!routing.IsEnd(index))
{
    Console.Write($"{manager.IndexToNode(index)} -> ");
    var previousIndex = index;
    index = solution.Value(routing.NextVar(index));
    routeCost += routing.GetArcCostForVehicle(previousIndex, index, 0);
}
Console.WriteLine(manager.IndexToNode(index));
Console.WriteLine($"Route cost: {routeCost}");