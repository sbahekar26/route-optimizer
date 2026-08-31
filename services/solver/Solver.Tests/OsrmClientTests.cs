using RouteOptimizer.Contracts;
using Solver;
using Xunit;

namespace Solver.Tests;

public class OsrmClientTests
{
    [Fact]
    public async Task GetTableAsync_ParsesDurations_AndBuildsLonLatUrl()
    {
        var fakeJson = """
        {
          "code": "Ok",
          "durations": [[0, 100], [110, 0]],
          "distances": [[0, 2000], [2100, 0]]
        }
        """;

        var handler = new FakeHttpMessageHandler(fakeJson);
        var http = new HttpClient(handler);
        var client = new OsrmClient(http, "http://osrm.test");

        var coordinates = new List<Coordinate>
        {
            new Coordinate(43.3255, -79.7990),  // lat, lon
            new Coordinate(43.6532, -79.3832),
        };

        var result = await client.GetTableAsync(coordinates);

        // parsed the durations matrix
        Assert.Equal(0, result.Durations[0][0]);
        Assert.Equal(100, result.Durations[0][1]);
        Assert.Equal(110, result.Durations[1][0]);

        // built the URL longitude-first (OSRM's order)
        Assert.Contains("-79.799,43.3255", handler.LastRequestUri);
    }
}
