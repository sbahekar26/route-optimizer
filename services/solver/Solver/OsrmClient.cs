using System.Text.Json;
using RouteOptimizer.Contracts;

namespace Solver;

public class OsrmClient
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;

    public OsrmClient(HttpClient http, string baseUrl)
    {
        _http = http;
        _baseUrl = baseUrl;
    }

    public async Task<OsrmTableResponse> GetTableAsync(IReadOnlyList<Coordinate> coordinates)
    {
        var coordString = string.Join(";",
            coordinates.Select(c => $"{c.Longitude},{c.Latitude}"));

        var url = $"{_baseUrl}/table/v1/driving/{coordString}?annotations=duration,distance";

        var json = await _http.GetStringAsync(url);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<OsrmTableResponse>(json, options)!;
    }

    public async Task<double[][]> GetRouteGeometryAsync(IReadOnlyList<Coordinate> orderedStops)
{
    var coordString = string.Join(";",
        orderedStops.Select(c => $"{c.Longitude},{c.Latitude}"));

    var url = $"{_baseUrl}/route/v1/driving/{coordString}?geometries=geojson&overview=full";

    var json = await _http.GetStringAsync(url);

    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var response = JsonSerializer.Deserialize<OsrmRouteResponse>(json, options)!;

    return response.Routes[0].Geometry.Coordinates;
}
}