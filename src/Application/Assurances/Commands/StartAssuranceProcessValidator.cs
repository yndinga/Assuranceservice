using FluentValidation;

namespace AssuranceService.Application.Assurances.Commands;

public class StartAssuranceProcessValidator : AbstractValidator<StartAssuranceProcessCommand>
{
    public StartAssuranceProcessValidator()
    {
        RuleFor(x => x.AssuranceId)
            .NotEmpty().WithMessage("L'identifiant de l'assurance est requis.");
    }
}
