using Microsoft.EntityFrameworkCore;
using StopsApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<StopsDbContext>(options => 
options.UseNpgsql(builder.Configuration.GetConnectionString("StopsDb")));

var app = builder.Build();

app.MapGet("/health", () => new { status = "healthy", service = "stops-api" });

app.Run();