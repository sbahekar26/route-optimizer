using Solver;
using Xunit;

namespace Solver.Tests;

public class RouteSolverTests
{
    [Fact]
    public void Solve_FourStopMatrix_ReturnsOptimalCost()
    {
        // Arrange
        long[,] matrix =
        {
            { 0, 10, 15, 20 },
            { 10, 0, 35, 25 },
            { 15, 35, 0, 30 },
            { 20, 25, 30, 0 },
        };
        var solver = new RouteSolver();

        // Act
        var result = solver.Solve(matrix);

        // Assert
        Assert.Equal(80, result.TotalCost);
    }
}