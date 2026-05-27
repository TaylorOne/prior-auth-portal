using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PriorAuthApi.Data;

namespace PriorAuthApi.Services;

public class DatabaseWarmupService(
    IServiceProvider services,
    ILogger<DatabaseWarmupService> log) : IHostedService
{
    public async Task StartAsync(CancellationToken hostShutdownToken)
    {
        log.LogInformation("Starting managed identity database pre-warm...");

        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        const int maxAttempts = 3;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var timeout = attempt == 1 ? TimeSpan.FromSeconds(90) : TimeSpan.FromSeconds(30);
            using var attemptCts = new CancellationTokenSource(timeout);
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(
                hostShutdownToken, attemptCts.Token);

            try
            {
                log.LogInformation("Pre-warm attempt {Attempt} of {Max}...", attempt, maxAttempts);
                await db.Database.ExecuteSqlRawAsync("SELECT 1", linked.Token);
                log.LogInformation("Pre-warm succeeded on attempt {Attempt}.", attempt);
                return;
            }
            catch (Exception ex) when (ex is SqlException or InvalidOperationException or OperationCanceledException)
            {
                if (hostShutdownToken.IsCancellationRequested)
                {
                    log.LogInformation("Pre-warm cancelled by host shutdown.");
                    return;
                }

                if (attempt == maxAttempts)
                {
                    log.LogWarning(ex, "Pre-warm exhausted {Max} attempts; relying on runtime retries.", maxAttempts);
                    return;
                }

                log.LogWarning("Pre-warm attempt {Attempt} failed: {Message}", attempt, ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(15), hostShutdownToken);
            }
        }
    }

    public Task StopAsync(CancellationToken _) => Task.CompletedTask;
}