using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using StopsApi;
using System.Text;
using System.Text.Json;
using RouteOptimizer.Contracts;

var builder = WebApplication.CreateBuilder(args);

// in Program.cs, after builder is created, before builder.Build()

var rabbitFactory = new ConnectionFactory { HostName = "localhost" };
var rabbitConnection = await rabbitFactory.CreateConnectionAsync();
builder.Services.AddSingleton(rabbitConnection);

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddDbContext<StopsDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("StopsDb")));

builder.Services.AddHostedService<ResultConsumer>();

var app = builder.Build();
app.UseCors("frontend");

app.MapGet("/health", () => new { status = "healthy", service = "stops-api" });

app.MapGet("/stops", async(StopsDbContext db) => 
    await db.Stops.ToListAsync());

app.MapGet("/optimize/{jobId}", async (Guid jobId, StopsDbContext db) =>
{
    var job = await db.OptimizationJobs.FindAsync(jobId);
    return job is null
        ? Results.NotFound(new { jobId, status = "pending or not found" })
        : Results.Ok(job);
});

app.MapPost("/stops", async (CreateStopRequest request, StopsDbContext db) =>
{
    var stop = new Stop
    {
        Address = request.Address,
        Latitude = request.Latitude,
        Longitude = request.Longitude,
        Status = StopStatus.Pending
    };

    db.Stops.Add(stop);
    await db.SaveChangesAsync();

    return Results.Created($"/stops/{stop.Id}", stop);
});

app.MapPost("/optimize", async (StopsDbContext db, IConnection rabbit) =>
{
    var stops = await db.Stops.ToListAsync();

    if (stops.Count < 2)
        return Results.BadRequest("Need at least 2 stops to optimize.");

    var coordinates = stops
        .Select(s => new Coordinate(s.Latitude, s.Longitude))
        .ToList();

    var jobId = Guid.NewGuid();
    var request = new OptimizationRequested(jobId, coordinates);

    var json = JsonSerializer.Serialize(request);
    var body = Encoding.UTF8.GetBytes(json);

    using var channel = await rabbit.CreateChannelAsync();
    await channel.QueueDeclareAsync(
        queue: "optimization-requests",
        durable: true,
        exclusive: false,
        autoDelete: false,
        arguments: null);

    await channel.BasicPublishAsync(
        exchange: "",
        routingKey: "optimization-requests",
        body: body);

    return Results.Accepted($"/optimize/{jobId}", new { jobId });
});

app.MapPut("/stops/{id}", async (Guid id, CreateStopRequest request, StopsDbContext db) =>
{
    var stop = await db.Stops.FindAsync(id);
    if (stop is null)
        return Results.NotFound();

    stop.Address = request.Address;
    stop.Latitude = request.Latitude;
    stop.Longitude = request.Longitude;

    await db.SaveChangesAsync();
    return Results.Ok(stop);
});

app.Run();