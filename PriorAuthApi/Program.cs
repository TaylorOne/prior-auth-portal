using Microsoft.EntityFrameworkCore;
using Azure.Messaging.ServiceBus;
using PriorAuthApi.Data;
using PriorAuthApi.Endpoints;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: [35]  // TCP Provider internal exception
        )
    )
);
    
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
                "https://yellow-glacier-0dd1a3010.7.azurestaticapps.net"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Pre-warm managed identity token on startup
if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
{
    _ = Task.Run(async () =>
    {
        for (int i = 0; i < 5; i++)
        {
            try
            {
                using var scope = app.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                await db.Database.ExecuteSqlRawAsync("SELECT 1", cts.Token);
                app.Logger.LogInformation("Managed identity pre-warm succeeded on attempt {Attempt}.", i + 1);
                return;
            }
            catch (Exception ex)
            {
                app.Logger.LogWarning(ex, "Managed identity pre-warm attempt {Attempt} failed. Retrying in {Delay}s.", i + 1, (i + 1) * 2);
                await Task.Delay(TimeSpan.FromSeconds((i + 1) * 2));
            }
        }
        app.Logger.LogWarning("Managed identity pre-warm failed after all attempts.");
    });
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
