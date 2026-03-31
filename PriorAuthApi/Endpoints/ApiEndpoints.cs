using Microsoft.EntityFrameworkCore;
using PriorAuthApi.Data;
using PriorAuthApi.Entities;

namespace PriorAuthApi.Endpoints
{
    public static class ApiEndpoints
    {
        public static void MapAuthRuleEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/authrules/{code}/{indicationCode}", async (AppDbContext db, string code, string indicationCode) =>
            {
                var authRule = await db.AuthRules
                    .Where(r => r.Code == code && r.IndicationCode == indicationCode && r.IsActive)
                    .FirstOrDefaultAsync();

                return authRule is not null ? Results.Ok(AuthRuleResponse.FromEntity(authRule)) : Results.NotFound();
            });

        }
    }
}