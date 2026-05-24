using Microsoft.EntityFrameworkCore;
using Azure.Messaging.ServiceBus;
using PriorAuthApi.Data;
using PriorAuthApi.Endpoints;
using System.Text.Json.Serialization;
using Azure.Identity;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
    {
        options.UseSqlServer(connectionString);
    }
    else
    {
        // Instantiating the credential once is completely thread-safe and highly efficient
        var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            ExcludeVisualStudioCredential = true,
            ExcludeInteractiveBrowserCredential = true
        });

        options.UseSqlServer(connectionString, sqlOptions => 
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: [35, 10054, 104]
            );
            sqlOptions.CommandTimeout(60);
        });

        // This is the real magic hook. It tells EF Core that every single time it opens 
        // a connection, run this code block to attach a fresh token from IMDS.
        options.AddInterceptors(new AzureSqlTokenInterceptor(credential));
    }
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
    app.Logger.LogInformation("Starting Managed Identity database pre-warm...");
    
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        int maxAttempts = 3;
        int delaySeconds = 15;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                app.Logger.LogInformation("Database pre-warm attempt {Attempt} of {MaxAttempts}...", attempt, maxAttempts);
                
                // Keep individual execution timeouts reasonable so we recover fast from server-side drops
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(25));
                await db.Database.ExecuteSqlRawAsync("SELECT 1", cts.Token);
                
                app.Logger.LogInformation("Database pre-warm completely succeeded on attempt {Attempt}. Connection pool is warm.", attempt);
                break; 
            }
            catch (Exception ex)
            {
                app.Logger.LogWarning("Pre-warm attempt {Attempt} encountered an issue: {Message}", attempt, ex.Message);
                
                if (attempt < maxAttempts)
                {
                    app.Logger.LogInformation("Waiting {Delay} seconds before retrying database connection...", delaySeconds);
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                }
                else
                {
                    app.Logger.LogCritical(ex, "All database pre-warm attempts exhausted. Proceeding to boot, relying on runtime connection retries.");
                }
            }
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
