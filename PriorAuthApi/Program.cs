using Microsoft.EntityFrameworkCore;
using Azure.Messaging.ServiceBus;
using PriorAuthApi.Data;
using PriorAuthApi.Services;
using PriorAuthApi.Endpoints;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.ExecutionStrategy(deps => new TransientLoginRetryStrategy(deps));
        sqlOptions.CommandTimeout(30);
    });
});
    
var serviceBusConnectionString = builder.Configuration.GetConnectionString("ServiceBus");
if (!string.IsNullOrEmpty(serviceBusConnectionString))
{
    builder.Services.AddSingleton(new ServiceBusClient(serviceBusConnectionString));
    builder.Services.AddSingleton(sp =>
        sp.GetRequiredService<ServiceBusClient>().CreateSender("auth-evaluation"));
}

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",      
                "https://yellow-glacier-0dd1a3010.7.azurest aticapps.net"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Pre-warm managed identity token on startup
if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddHostedService<DatabaseWarmupService>();
}

app.MapOpenApi();

app.UseCors("DevCors");

app.UseExceptionHandler(exceptionApp => exceptionApp.Run(async context =>
{
    var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    if (error is BadHttpRequestException or System.Text.Json.JsonException)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new { Error = "Invalid request body. Please check that all fields contain valid values." });
    }
}));

app.MapAuthRuleEndpoints();

app.MapGet("/health", () => Results.Ok(new { status = "alive", time = DateTime.UtcNow }))
   .WithTags("Health")
   .AllowAnonymous();

app.MapGet("/health/db", async (AppDbContext db) =>
{
    try
    {
        await db.Database.ExecuteSqlRawAsync("SELECT 1");
        return Results.Ok(new { status = "alive", db = "connected", time = DateTime.UtcNow });
    }
    catch
    {
        return Results.Json(new { status = "degraded", db = "unavailable" }, 
            statusCode: 503);
    }
})
.WithTags("Health")
.AllowAnonymous();

app.Run();

public partial class Program { }
