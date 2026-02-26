using FluentValidation;

namespace AssuranceService.Application.Assurances.Commands;

public class DeleteAssuranceValidator : AbstractValidator<DeleteAssuranceCommand>
{
    public DeleteAssuranceValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("L'identifiant de l'assurance est requis.");
    }
}
