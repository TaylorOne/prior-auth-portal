using Microsoft.EntityFrameworkCore;
using Azure.Messaging.ServiceBus;
using System.Text.Json;
using PriorAuth.Contracts;
using PriorAuth.Data;
using PriorAuth.Data.Entities;
using PriorAuth.Data.Services;
using PriorAuthApi.DTOs;
using PriorAuthApi.Validators;
using PriorAuthApi.Services;

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
            })
            .RequireAuthorization("PrescriberOnly");
            
            app.MapGet("/authrules/codes", async (AppDbContext db) =>
            {
                var codes = await db.AuthRules
                    .Where(r => r.IsActive)
                    .Select(r => new { r.Code, r.CodeSystem, r.DisplayName })
                    .Distinct()
                    .ToListAsync();

                return Results.Ok(codes);
            })
            .WithName("GetAuthRuleCodes")
            .RequireAuthorization("PrescriberOnly");

            app.MapGet("/authrules/{code}/indications", async (AppDbContext db, string code) =>
            {
                var indications = await db.AuthRules
                    .Where(r => r.Code == code && r.IsActive)
                    .Select(r => new IndicationDto(r.IndicationCode, r.IndicationDisplayName))
                    .ToListAsync();

                return Results.Ok(indications);
            })
            .WithName("GetAuthRuleIndications")
            .RequireAuthorization("PrescriberOnly");

            app.MapGet("/priorauth", async (AppDbContext db) =>
            {
                var requests = await db.PriorAuthRequests
                    .Include(r => r.Patient)
                    .Include(r => r.Practitioner)
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new PriorAuthSummaryDto(
                        r.Id,
                        r.Status.ToString(),
                        r.Priority,
                        r.ServiceCode,
                        r.ServiceCodeDisplay ?? r.ServiceCode,
                        $"{r.Patient.FirstName} {r.Patient.LastName}",
                        $"Dr. {r.Practitioner.FirstName} {r.Practitioner.LastName}",
                        r.Practitioner.Specialty,
                        r.CreatedAt,
                        r.DeterminationDate,
                        r.EvaluationReason,
                        r.ReviewerNotes
                    ))
                    .ToListAsync();

                return Results.Ok(requests);
            })
            .WithName("GetPriorAuthRequests")
            .RequireAuthorization("PrescriberOnly");

            app.MapGet("/priorauth/{id:int}", async (AppDbContext db, int id) =>
            {
                var r = await db.PriorAuthRequests
                    .Include(r => r.Patient)
                    .Include(r => r.Practitioner)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (r is null) return Results.NotFound();

                return Results.Ok(new PriorAuthDetailDto(
                    r.Id,
                    r.Status.ToString(),
                    $"{r.Patient.FirstName} {r.Patient.LastName}",
                    $"Dr. {r.Practitioner.FirstName} {r.Practitioner.LastName}",
                    r.ServiceCode,
                    r.ServiceCodeDisplay ?? r.ServiceCode,
                    r.CreatedAt,
                    r.ClinicalData
                ));
            })
            .WithName("GetPriorAuthDetail")
            .RequireAuthorization("ReviewerOnly");

            app.MapPost("/priorauth", async (
                AppDbContext db,
                ServiceBusSender sender,
                AuditService audit,
                IPractitionerResolver resolver,
                CancellationToken ct,
                SubmitPriorAuthDto dto) =>
            {
                if (dto.Code == null || string.IsNullOrEmpty(dto.Code.Code))
                {
                    return Results.BadRequest("Service code is required.");
                }

                if (dto.ReasonCode == null || !dto.ReasonCode.Any())
                {
                    return Results.BadRequest("At least one reason code is required.");
                }

                var indicationCode = dto.ReasonCode[0];

                var authRule = await db.AuthRules
                    .Where(r => r.Code == dto.Code.Code && r.IndicationCode == indicationCode && r.IsActive)
                    .FirstOrDefaultAsync();

                if (authRule is null)
                {
                    return Results.BadRequest("No applicable authorization rule found for the provided code and indication.");
                }

                var result = new PriorAuthRequestValidator(authRule).Validate(dto);
                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(e => e.ErrorMessage).ToList();
                    return Results.BadRequest(new { Errors = errors });
                }

                var patientExists = await db.Patients.AnyAsync(p => p.Id == dto.PatientId);
                if (!patientExists)                {
                    return Results.BadRequest($"Patient with ID {dto.PatientId} does not exist.");
                }

                var practitioner = await resolver.ResolveCurrentAsync(ct);
                if (practitioner is null)
                    return Results.Problem(
                        "Authenticated user is not linked to a practitioner record.",
                        statusCode: StatusCodes.Status403Forbidden);

                var request = new PriorAuthRequest
                {
                    ServiceCode = dto.Code.Code,
                    ServiceCodeSystem = dto.Code.System,
                    ServiceCodeDisplay = dto.Code.Display,
                    Priority = dto.Priority,
                    Status = Status.Submitted,
                    PatientId = dto.PatientId,
                    PractitionerId = practitioner.Id,
                    ClinicalData = dto.ClinicalData is not null
                        ? JsonSerializer.Serialize(dto.ClinicalData)
                        : string.Empty,
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

                await audit.LogAsync(request.Id, AuditEventTypes.RequestCreated, AuditActors.System, new
                {
                    patientId = request.PatientId,
                    practitionerId = request.PractitionerId,
                    serviceCode = request.ServiceCode,
                    priority = request.Priority
                });

                var correlationId = Guid.NewGuid().ToString();
                var message = new ServiceBusMessage(JsonSerializer.Serialize(new PriorAuthSubmittedMessage
                {
                    PriorAuthRequestId = request.Id
                }))
                {
                    CorrelationId = correlationId
                };

                // NOTE: SaveChangesAsync and SendMessageAsync are not atomic. If the Service Bus send fails,
                // the request is persisted but never evaluated. In production this would be addressed with
                // the transactional outbox pattern — persisting the message to the DB in the same transaction
                // as the request, then delivering it to the bus via a background process.
                // See ADR-007 for the decision record.
                await sender.SendMessageAsync(message);

                await audit.LogAsync(request.Id, AuditEventTypes.MessagePublished, AuditActors.System, new
                {
                    correlationId,
                    queue = "auth-evaluation"
                });

                return Results.Created($"/priorauth/{request.Id}", new { request.Id });
            })
            .WithName("SubmitPriorAuth")
            .RequireAuthorization("PrescriberOnly");

            app.MapPatch("/priorauth/{id}/decision", async (AppDbContext db, AuditService audit, int id, ReviewDecisionDto dto) =>
            {
                if (dto.Decision != "Approved" && dto.Decision != "Denied")
                    return Results.BadRequest("Decision must be 'Approved' or 'Denied'.");

                var request = await db.PriorAuthRequests.FindAsync(id);
                if (request is null)
                    return Results.NotFound();

                if (request.Status != Status.UnderReview)
                    return Results.BadRequest($"Request is not under review (current status: {request.Status}).");

                var fromStatus = request.Status.ToString();
                request.Status = dto.Decision == "Approved" ? Status.Approved : Status.Denied;
                request.ReviewerNotes = dto.ReviewerNotes;
                request.DeterminationDate = DateTime.UtcNow;
                request.UpdatedAt = DateTime.UtcNow;

                await db.SaveChangesAsync();

                await audit.LogAsync(id, AuditEventTypes.StatusTransitioned, AuditActors.Reviewer,
                    new { from = fromStatus, to = request.Status.ToString() });
                await audit.LogAsync(id, AuditEventTypes.ManualReviewDecision, AuditActors.Reviewer,
                    new { decision = request.Status.ToString(), reviewerNotes = dto.ReviewerNotes });

                return Results.Ok(new { request.Id, Status = request.Status.ToString(), request.DeterminationDate });
            })
            .WithName("ReviewDecision")
            .RequireAuthorization("ReviewerOnly");

            app.MapGet("/priorauth/review-queue", async (AppDbContext db) =>
            {
                var requests = await db.PriorAuthRequests
                    .Where(r => r.Status == Status.UnderReview)
                    .Include(r => r.Patient)
                    .Include(r => r.Practitioner)
                    .OrderBy(r => r.CreatedAt)
                    .Select(r => new {
                        r.Id,
                        r.Status,
                        PatientName = r.Patient.FirstName + " " + r.Patient.LastName,
                        PractitionerName = r.Practitioner.FirstName + " " + r.Practitioner.LastName,
                        r.ServiceCode,
                        r.ServiceCodeDisplay,
                        r.CreatedAt,
                        r.ClinicalData
                    })
                    .ToListAsync();

                return Results.Ok(requests);
            })
            .WithName("GetReviewQueue")
            .RequireAuthorization("ReviewerOnly");

            app.MapGet("/patients", async (AppDbContext db) =>
            {
                var patients = await db.Patients
                    .Select(p => new PatientSummaryDto(
                        p.Id,
                        $"{p.FirstName} {p.LastName}",
                        DateTime.UtcNow.Year - p.DateOfBirth.Year -
                            (new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day)
                                < p.DateOfBirth ? 1 : 0),
                        p.Gender.ToString()
                    ))
                    .ToListAsync();

                return Results.Ok(patients);
            })
            .WithName("GetPatients")
            .RequireAuthorization("PrescriberOnly");

            app.MapGet("/practitioners", async (AppDbContext db) =>
            {
                var practitioners = await db.Practitioners
                    .Select(p => new PractitionerSummaryDto(
                        p.Id,
                        $"Dr. {p.FirstName} {p.LastName}",
                        p.Npi,
                        p.Specialty
                    ))
                    .ToListAsync();

                return Results.Ok(practitioners);
            })
            .WithName("GetPractitioners")
            .RequireAuthorization("PrescriberOnly");

            app.MapGet("/practitioners/me", async (IPractitionerResolver resolver, CancellationToken ct) =>
            {
                var practitioner = await resolver.ResolveCurrentAsync(ct);
                if (practitioner is null)
                    return Results.Problem(
                        "Authenticated user is not linked to a practitioner record.",
                        statusCode: StatusCodes.Status403Forbidden);

                var dto = new PractitionerSummaryDto(
                    practitioner.Id,
                    $"Dr. {practitioner.FirstName} {practitioner.LastName}",
                    practitioner.Npi,
                    practitioner.Specialty
                );

                return Results.Ok(dto);
            })
            .WithName("GetCurrentPractitioner")
            .RequireAuthorization("PrescriberOnly");
            
        }
    }
}