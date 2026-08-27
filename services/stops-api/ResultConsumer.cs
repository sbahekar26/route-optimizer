using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RouteOptimizer.Contracts;

namespace StopsApi;

public class ResultConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IServiceProvider _services;

    public ResultConsumer(IConnection connection, IServiceProvider services)
    {
        _connection = connection;
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: "optimization-results",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var result = JsonSerializer.Deserialize<RouteOptimized>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<StopsDbContext>();

            db.OptimizationJobs.Add(new OptimizationJob
            {
                JobId = result.JobId,
                Route = result.Route,
                TotalCost = result.TotalCost,
                CompletedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            Console.WriteLine($"Stored result for job {result.JobId}");
        };

        await channel.BasicConsumeAsync(
            queue: "optimization-results",
            autoAck: true,
            consumer: consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
