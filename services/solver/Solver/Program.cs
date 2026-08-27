using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RouteOptimizer.Contracts;
using Solver;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(
    queue: "optimization-requests",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null);

await channel.QueueDeclareAsync(
    queue: "optimization-results",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null);

using var http = new HttpClient();
var osrm = new OsrmClient(http, "http://localhost:5050");
var solver = new RouteSolver();

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += async (model, ea) =>
{
    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
    var request = JsonSerializer.Deserialize<OptimizationRequested>(json,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

    Console.WriteLine($"Received job {request.JobId} with {request.Stops.Count} stops");

    var table = await osrm.GetTableAsync(request.Stops);
    var matrix = ToLongMatrix(table.Durations);
    var result = solver.Solve(matrix);

    var response = new RouteOptimized(request.JobId, result.Route, result.TotalCost);
    var responseJson = JsonSerializer.Serialize(response);
    var responseBody = Encoding.UTF8.GetBytes(responseJson);

    await channel.BasicPublishAsync(
        exchange: "",
        routingKey: "optimization-results",
        body: responseBody);

    Console.WriteLine($"Published result for job {request.JobId}: cost {result.TotalCost}s");

    Console.WriteLine($"Job {request.JobId}: route {string.Join(" -> ", result.Route)}, cost {result.TotalCost}s");
};


await channel.BasicConsumeAsync(queue: "optimization-requests", autoAck: true, consumer: consumer);

Console.WriteLine("Solver worker running. Waiting for jobs. Press Enter to exit.");
Console.ReadLine();

static long[,] ToLongMatrix(double[][] source)
{
    int n = source.Length;
    var matrix = new long[n, n];
    for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++)
            matrix[i, j] = (long)source[i][j];
    return matrix;
}