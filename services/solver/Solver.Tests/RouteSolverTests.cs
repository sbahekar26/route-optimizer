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

    [Fact]
    public void HaversineDistance_BurlingtonToToronto_ReturnsRoughly50km()
    {
        // Arrange
        var burlington = new Coordinate(43.3255, -79.7990);
        var toronto = new Coordinate(43.6532, -79.3832);

        // Act
        long distance = DistanceCalculator.HaversineDistance(burlington, toronto);

        // Assert
        Assert.InRange(distance, 48_000, 52_000);
    }

        [Fact]
    public void BuildMatrix_TwoCoordinates_IsSymmetricWithZeroDiagonal()
    {
        // Arrange
        var coordinates = new List<Coordinate>
        {
            new Coordinate(43.3255, -79.7990),  // Burlington
            new Coordinate(43.6532, -79.3832),  // Toronto
        };

        // Act
        var matrix = DistanceCalculator.BuildMatrix(coordinates);

        // Assert
        Assert.Equal(0, matrix[0, 0]);              // point to itself = 0
        Assert.Equal(0, matrix[1, 1]);              // point to itself = 0
        Assert.Equal(matrix[0, 1], matrix[1, 0]);   // same distance both ways
        Assert.InRange(matrix[0, 1], 48_000, 52_000); // ~50km apart
    }
}