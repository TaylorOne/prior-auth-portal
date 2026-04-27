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
    options.UseSqlServer(connectionString));
    
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
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // await AuthRuleSeeder.SeedAsync(context);
    // await OrganizationSeeder.SeedAsync(context);
    // await PractitionerSeeder.SeedAsync(context);
    // wait PatientSeeder.SeedAsync(context);
}

app.MapOpenApi();

app.UseCors("DevCors");
app.UseHttpsRedirection();

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

app.MapGet("/health", () => Results.Ok(new { status = "alive", time = DateTime.UtcNow }));

app.Run();

public partial class Program { }
