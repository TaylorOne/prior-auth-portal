using System.Data;
using FluentValidation;
using PriorAuthApi.DTOs;

namespace PriorAuthApi.Validators
{
    public class MedicationRequestValidator : AbstractValidator<MedicationRequestDto>
    {
        public MedicationRequestValidator()
        {
            RuleFor(x => x.Medication)
                .NotNull()
                .WithMessage("Medication code must be provided.")
                .SetValidator(new CodeableConceptValidator());

            RuleFor(x => x.Intent)
                .NotEmpty()
                .WithMessage("Intent must be provided.")
                .Must(i => i == "order")
                .WithMessage("Intent must be 'order' for PA submissions.");

            RuleFor(x => x.DosageInstructionText)
                .NotEmpty()
                .WithMessage("Dosage instructions cannot be empty.");

            RuleFor(x => x.QuantityValue)
                .NotEmpty()
                .WithMessage("Quantity cannot be empty.")
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than zero.");
            
            RuleFor(x => x.QuantityUnit)
                .NotEmpty()
                .WithMessage("Quantity unit must be provided.")
                .Must(p => p == "vial" || p == "syringe" || p == "pen" || p == "tablet" || p == "capsule")
                .WithMessage("Quantity unit must be a valid string.");
            
            RuleFor(x => x.NumberOfRepeatsAllowed)
                .NotEmpty()
                .WithMessage("Number of repeats allowed cannot be empty.")
                .GreaterThanOrEqualTo(0)
                .WithMessage("Number of repeats allowed must be zero or greater.");
            
            RuleFor(x => x.ExpectedSupplyDurationDays)
                .NotEmpty()
                .WithMessage("Expected supply duration cannot be empty.")
                .GreaterThan(0)
                .WithMessage("Expected supply duration must be greater than zero.");
        }
    }
}