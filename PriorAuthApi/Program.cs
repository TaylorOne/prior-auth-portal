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
    var sqlConnection = new SqlConnection(connectionString);
    
    // If running in Azure, explicitly inject the token before passing connection to EF Core
    if (!builder.Environment.IsDevelopment() && !builder.Environment.IsEnvironment("Testing"))
    {
        var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            // Speed up cold starts by ignoring credentials you aren't using locally
            ExcludeSharedTokenCacheCredential = true,
            ExcludeVisualStudioCredential = true,
            ExcludeInteractiveBrowserCredential = true
        });
        
        // Request token synchronously or handle natively via Microsoft.Data.SqlClient callback
        sqlConnection.AccessToken = credential.GetToken(
            new Azure.Core.TokenRequestContext(new[] { "https://database.windows.net/.default" })
        ).Token;
    }

    options.UseSqlServer(sqlConnection, sqlOptions => 
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: [35, 10054, 104]
        );
        sqlOptions.CommandTimeout(60);
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
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(45));
            await db.Database.ExecuteSqlRawAsync("SELECT 1", cts.Token);
            app.Logger.LogInformation("Database pre-warm succeeded. Connection pool is warm.");
        }
        catch (Exception ex)
        {
            app.Logger.LogCritical(ex, "Database pre-warm failed. Proceeding to boot, relying on connection-level retries.");
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
