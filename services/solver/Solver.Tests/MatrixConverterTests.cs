using Solver;
using Xunit;

namespace Solver.Tests;

public class MatrixConverterTests
{
    [Fact]
    public void ToLongMatrix_ConvertsJaggedToRectangular_AndTruncates()
    {
        double[][] source =
        {
            new double[] { 0.0, 10.9, 20.1 },
            new double[] { 11.5, 0.0, 30.7 },
            new double[] { 21.2, 31.8, 0.0 },
        };

        var result = MatrixConverter.ToLongMatrix(source);

        Assert.Equal(3, result.GetLength(0));
        Assert.Equal(3, result.GetLength(1));
        Assert.Equal(0, result[0, 0]);
        Assert.Equal(10, result[0, 1]);   // 10.9 truncated to 10
        Assert.Equal(30, result[1, 2]);   // 30.7 truncated to 30
        Assert.Equal(21, result[2, 0]);   // 21.2 truncated to 21
    }
}
