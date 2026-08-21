namespace Solver;

public static class DistanceCalculator
{
    private const double EarthRadiusMeters = 6_371_000;

    public static long HaversineDistance(Coordinate from, Coordinate to)
    {
        double lat1 = ToRadians(from.Latitude);
        double lat2 = ToRadians(to.Latitude);
        double deltaLat = ToRadians(to.Latitude - from.Latitude);
        double deltaLon = ToRadians(to.Longitude - from.Longitude);

        double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2)
                 + Math.Cos(lat1) * Math.Cos(lat2)
                 * Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return (long)(EarthRadiusMeters * c);
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
public static long[,] BuildMatrix(IReadOnlyList<Coordinate> coordinates)
{
    int n = coordinates.Count;
    var matrix = new long[n, n];

    for (int i = 0; i < n; i++)
    {
        for (int j = 0; j < n; j++)
        {
            matrix[i, j] = HaversineDistance(coordinates[i], coordinates[j]);
        }
    }

    return matrix;
}

}