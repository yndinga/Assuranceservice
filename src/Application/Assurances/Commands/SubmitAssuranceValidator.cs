using FluentValidation;

namespace AssuranceService.Application.Assurances.Commands;

public class SubmitAssuranceValidator : AbstractValidator<SubmitAssuranceCommand>
{
    public SubmitAssuranceValidator()
    {
        RuleFor(x => x.AssuranceId)
            .NotEmpty().WithMessage("L'identifiant de l'assurance est requis.");
    }
}
