using Microsoft.EntityFrameworkCore;
using PriorAuthApi.Data;
using PriorAuthApi.Entities;
using PriorAuthApi.DTOs;

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

                return authRule is not null ? Results.Ok(AuthRuleResponseDto.FromEntity(authRule)) : Results.NotFound();
            });

            app.MapPost("/priorauth", async (AppDbContext db, SubmitPriorAuthDto dto) =>
            {
                if (dto.ReasonCode is null || !dto.ReasonCode.Any())
                {
                    return Results.BadRequest("At least one reason code must be provided.");
                }

                var patientExists = await db.Patients.AnyAsync(p => p.Id == dto.PatientId);
                if (!patientExists)                {
                    return Results.BadRequest($"Patient with ID {dto.PatientId} does not exist.");
                }

                var practitionerExists = await db.Practitioners.AnyAsync(p => p.Id == dto.PractitionerId);
                if (!practitionerExists)
                {
                    return Results.BadRequest($"Practitioner with ID {dto.PractitionerId} does not exist.");
                }

                var indicationCode = dto.ReasonCode[0].Code;

                var authRule = await db.AuthRules
                    .Where(r => r.Code == dto.Code.Code && r.IndicationCode == indicationCode && r.IsActive)
                    .FirstOrDefaultAsync();

                if (authRule is null)
                {
                    return Results.BadRequest("No applicable authorization rule found for the provided code and indication.");
                }

                var request = new PriorAuthRequest
                {
                    ServiceCode = dto.Code.Code,
                    ServiceCodeSystem = dto.Code.System,
                    ServiceCodeDisplay = dto.Code.Display,
                    Priority = dto.Priority,
                    Status = Status.Draft,
                    PatientId = dto.PatientId,
                    PractitionerId = dto.PractitionerId,
                    ClinicalData = dto.ClinicalData is not null
                        ? System.Text.Json.JsonSerializer.Serialize(dto.ClinicalData)
                        : null,
                    AuthRuleId = authRule.Id,
                    CreatedAt = DateTime.UtcNow
                };

                if (dto.MedicationRequest is not null)
                {
                    request.MedicationRequest = new MedicationRequest
                    {
                        MedicationCode = dto.MedicationRequest.Medication.Code,
                        MedicationSystem = dto.MedicationRequest.Medication.System,
                        MedicationDisplay = dto.MedicationRequest.Medication.Display,
                        DosageInstructionText = dto.MedicationRequest.DosageInstructionText,
                        QuantityValue = dto.MedicationRequest.QuantityValue,
                        QuantityUnit = dto.MedicationRequest.QuantityUnit,
                        NumberOfRepeatsAllowed = dto.MedicationRequest.NumberOfRepeatsAllowed,
                        ExpectedSupplyDurationDays = dto.MedicationRequest.ExpectedSupplyDurationDays,
                    };
                }

                db.PriorAuthRequests.Add(request);
                await db.SaveChangesAsync();

                return Results.Created($"/priorauth/{request.Id}", new { request.Id });
            })
            .WithName("SubmitPriorAuth");
        }
    }
}