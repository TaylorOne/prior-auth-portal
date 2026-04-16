using System.Text.Json;
using FluentValidation;
using PriorAuthApi.DTOs;
using PriorAuthApi.Entities;

namespace PriorAuthApi.Validators
{
    public class PriorAuthRequestValidator : AbstractValidator<SubmitPriorAuthDto>
    {
        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public PriorAuthRequestValidator(AuthRule authRule)
        {
            RuleFor(x => x.Priority)
                .NotEmpty()
                .Must(p => p == "routine" || p == "urgent")
                .WithMessage("Priority must be either 'routine' or 'urgent'.");

            RuleFor(x => x.Code)
                .NotNull()
                .WithMessage("A valid service code with system must be provided.")
                .SetValidator(new CodeableConceptValidator());

            RuleFor(x => x.PatientId)
                .NotNull()
                .GreaterThan(0)
                .WithMessage("PatientId must be a positive integer.");

            RuleFor(x => x.PractitionerId)
                .NotNull()
                .GreaterThan(0)
                .WithMessage("PractitionerId must be a positive integer.");

            RuleForEach(x => x.ReasonCode)
                .NotEmpty()
                .WithMessage("Reason codes cannot be empty.");
            
            var formDef = JsonSerializer.Deserialize<FormDefinition>(authRule.FormDefinition, _jsonOptions) ?? new FormDefinition();

            foreach (var field in formDef.Fields)
            {
                RuleFor(x => x.ClinicalData)
                    .Must(clinicalData =>
                    {
                        if (clinicalData == null || !clinicalData.ContainsKey(field.Name))
                        {
                            return !field.Validation.Required;
                        }

                        var value = clinicalData[field.Name];
                        if (field.Validation.AllowedValues != null && !field.Validation.AllowedValues.Contains(value.ToString()))
                        {
                            return false;
                        }

                        if (field.Type == "number")
                        {
                            if (!double.TryParse(value.ToString(), out double numValue))
                            {
                                return false;
                            }
                            if (field.Validation.Min.HasValue && numValue < field.Validation.Min.Value)
                            {
                                return false;
                            }
                            if (field.Validation.Max.HasValue && numValue > field.Validation.Max.Value)
                            {
                                return false;
                            }
                        }

                        if (field.Validation.Integer == true && !int.TryParse(value.ToString(), out _))
                        {
                            return false;
                        }

                        if (field.Validation.MaxLength.HasValue && value.ToString().Length > field.Validation.MaxLength.Value)
                        {
                            return false;
                        }

                        return true;
                    })
                    .WithMessage($"Field '{field.Name}' is invalid.");
            }

            When(x => x.MedicationRequest != null, () =>
            {
                RuleFor(x => x.MedicationRequest!)
                    .SetValidator(new MedicationRequestValidator());
            });
        }
    }
}