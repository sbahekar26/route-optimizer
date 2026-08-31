namespace Solver;

public static class MatrixConverter
{
    public static long[,] ToLongMatrix(double[][] source)
    {
        int n = source.Length;
        var matrix = new long[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                matrix[i, j] = (long)source[i][j];
        return matrix;
    }
}
