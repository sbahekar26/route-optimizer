using Google.OrTools.ConstraintSolver;
using RouteOptimizer.Contracts;

namespace Solver;

public record RouteResult(long[] Route, long TotalCost);

public class RouteSolver
{
    public RouteResult Solve(long[,] distanceMatrix)
    {
        int vehicleCount = 1;
        int depot = 0;

        var manager = new RoutingIndexManager(
            distanceMatrix.GetLength(0), vehicleCount, depot);
        var routing = new RoutingModel(manager);

        int transitCallbackIndex = routing.RegisterTransitCallback((fromIndex, toIndex) =>
        {
            var fromNode = manager.IndexToNode(fromIndex);
            var toNode = manager.IndexToNode(toIndex);
            return distanceMatrix[fromNode, toNode];
        });

        routing.SetArcCostEvaluatorOfAllVehicles(transitCallbackIndex);

        var searchParameters =
            operations_research_constraint_solver.DefaultRoutingSearchParameters();
        searchParameters.FirstSolutionStrategy =
            FirstSolutionStrategy.Types.Value.PathCheapestArc;

        var solution = routing.SolveWithParameters(searchParameters);

        // build the route list + cost, then RETURN it (don't print)
        var route = new List<long>();
        var index = routing.Start(0);
        while (!routing.IsEnd(index))
        {
            route.Add(manager.IndexToNode(index));
            index = solution.Value(routing.NextVar(index));
        }
        route.Add(manager.IndexToNode(index));

        return new RouteResult(route.ToArray(), solution.ObjectiveValue());
    }
}