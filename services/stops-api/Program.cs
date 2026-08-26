using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using StopsApi;

var builder = WebApplication.CreateBuilder(args);

// in Program.cs, after builder is created, before builder.Build()

var rabbitFactory = new ConnectionFactory { HostName = "localhost" };
var rabbitConnection = await rabbitFactory.CreateConnectionAsync();
builder.Services.AddSingleton(rabbitConnection);

builder.Services.AddDbContext<StopsDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("StopsDb")));

var app = builder.Build();

app.MapGet("/health", () => new { status = "healthy", service = "stops-api" });

app.MapGet("/stops", async(StopsDbContext db) => 
    await db.Stops.ToListAsync());

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

app.Run();