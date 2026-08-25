using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RouteOptimizer.Contracts;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(
    queue: "optimization-requests",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null);

var request = new OptimizationRequested(
    Guid.NewGuid(),
    new List<Coordinate>
    {
        new Coordinate(43.3255, -79.7990),  // Burlington
        new Coordinate(43.6532, -79.3832),  // Toronto
        new Coordinate(43.5890, -79.6441),  // Oakville
        new Coordinate(43.4675, -79.6877),  // Bronte
    });

var json = JsonSerializer.Serialize(request);
var body = Encoding.UTF8.GetBytes(json);

await channel.BasicPublishAsync(
    exchange: "",
    routingKey: "optimization-requests",
    body: body);

Console.WriteLine($"Published job {request.JobId}");