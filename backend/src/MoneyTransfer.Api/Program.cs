using Microsoft.EntityFrameworkCore;
using MoneyTransfer.Api.Data;
using MoneyTransfer.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Demo mode: set UseInMemoryDatabase=true to run the API without SQL Server
// (data lives in process memory and is lost on restart). Used for quick local
// demos; the docker compose setup always uses SQL Server.
if (builder.Configuration.GetValue<bool>("UseInMemoryDatabase"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("MoneyTransfer"));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
}

builder.Services.AddScoped<ITransferService, TransferService>();

// Only needed when the frontend dev server (vite, port 5173) talks to the API
// directly. In the docker compose setup nginx proxies /api → same origin.
const string DevCorsPolicy = "DevCors";
builder.Services.AddCors(options => options.AddPolicy(DevCorsPolicy, policy =>
    policy.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.UseCors(DevCorsPolicy);
}

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

await DbInitializer.InitializeAsync(app.Services);

app.Run();

public partial class Program; // exposes the entry point to integration tests
