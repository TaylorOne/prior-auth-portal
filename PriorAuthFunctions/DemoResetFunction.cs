    using Microsoft.Azure.Functions.Worker;
    using Microsoft.Azure.Functions.Worker.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using PriorAuth.Data;
    using System.Net;

    namespace PriorAuthFunctions;

    public class DemoResetFunction
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DemoResetFunction> _logger;

        public DemoResetFunction(AppDbContext context, ILogger<DemoResetFunction> logger)
        {
            _context = context;
            _logger = logger;
        }

        [Function("DemoResetTimer")]
        public async Task RunTimer([TimerTrigger("0 0 0 * * *")] TimerInfo timer)
        {
            _logger.LogInformation("Demo reset triggered by timer at {Time}", DateTime.UtcNow);
            await ExecuteReset();
        }

        [Function("DemoResetHttp")]
        public async Task<HttpResponseData> RunHttp(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("Demo reset triggered manually at {Time}", DateTime.UtcNow);
            await ExecuteReset();
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("Demo reset complete.");
            return response;
        }

        private async Task ExecuteReset()
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            _logger.LogInformation("Starting demo data wipe...");

            // Delete in FK-safe order (children before parents)
            await _context.MedicationRequests.ExecuteDeleteAsync();
            await _context.PriorAuthRequests.ExecuteDeleteAsync();
            await _context.Patients.ExecuteDeleteAsync();
            await _context.Practitioners.ExecuteDeleteAsync();
            await _context.Organizations.ExecuteDeleteAsync();
            await _context.AuthRules.ExecuteDeleteAsync();

            _logger.LogInformation("Wipe complete. Re-seeding...");

            // Seed in dependency order (parents before children)
            await AuthRuleSeeder.SeedAsync(_context, msg => _logger.LogInformation(msg));
            await OrganizationSeeder.SeedAsync(_context);
            await PractitionerSeeder.SeedAsync(_context);
            await PatientSeeder.SeedAsync(_context);

            await transaction.CommitAsync();
            _logger.LogInformation("Demo reset complete.");
        }
    }