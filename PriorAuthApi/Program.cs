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
        sqlOptions => {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,                  // Increased retries
                maxRetryDelay: TimeSpan.FromSeconds(5), // Shorter delay intervals to catch it quicker
                errorNumbersToAdd: [35, 10054, 104]     // Added common socket reset error numbers
            );
            sqlOptions.CommandTimeout(60);         // Give cheap SQL tiers time to respond
        }
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
    app.Logger.LogInformation("Starting synchronous Managed Identity token pre-warm...");
    
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        bool warmedUp = false;
        int maxAttempts = 4;

        for (int i = 0; i < maxAttempts; i++)
        {
            try
            {
                // Give the execution a generous 45 seconds to fetch token + talk to database
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(45));
                
                app.Logger.LogInformation("Database pre-warm token attempt {Attempt} initiated.", i + 1);
                await db.Database.ExecuteSqlRawAsync("SELECT 1", cts.Token);
                
                app.Logger.LogInformation("Managed identity pre-warm completely succeeded on attempt {Attempt}.", i + 1);
                warmedUp = true;
                break; 
            }
            catch (Exception ex)
            {
                app.Logger.LogWarning("Pre-warm attempt {Attempt} failed: {Message}. Retrying...", i + 1, ex.Message);
                
                if (i < maxAttempts - 1)
                {
                    // Linear backoff instead of compounding multiplication to save time
                    await Task.Delay(TimeSpan.FromSeconds(5)); 
                }
            }
        }

        if (!warmedUp)
        {
            app.Logger.LogCritical("CRITICAL: Managed identity pre-warm failed completely. App stepping down to allow platform recovery.");
            // Exiting gracefully here tells Azure App Service your container is broken, 
            // prompting a clean restart rather than leaving a zombie running.
            Environment.Exit(1); 
        }
    }
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
