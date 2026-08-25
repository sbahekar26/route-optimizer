using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory { HostName = "localhost" };

using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(
    queue: "hello",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null);

// --- consume ---
var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($"Received: {message}");
    return Task.CompletedTask;
};

await channel.BasicConsumeAsync(queue: "hello", autoAck: true, consumer: consumer);

// --- publish ---
var text = "hello from the solver";
var bodyBytes = Encoding.UTF8.GetBytes(text);
await channel.BasicPublishAsync(exchange: "", routingKey: "hello", body: bodyBytes);
Console.WriteLine($"Sent: {text}");

Console.WriteLine("Press Enter to exit.");
Console.ReadLine();