using FluentValidation;
using PriorAuthApi.DTOs;

namespace PriorAuthApi.Validators
{
    public class CodeableConceptValidator : AbstractValidator<CodeableConceptDto>
    {
        public CodeableConceptValidator()
        {
            RuleFor(x => x.System)
                .NotEmpty()
                .WithMessage("System is required.");
            RuleFor(x => x.Code)
                .NotEmpty()
                .WithMessage("Service code is required.");
        }
    }
}