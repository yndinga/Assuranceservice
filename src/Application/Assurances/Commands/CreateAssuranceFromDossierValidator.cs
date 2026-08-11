using FluentValidation;

namespace AssuranceService.Application.Assurances.Commands;

public sealed class CreateAssuranceFromDossierValidator : AbstractValidator<CreateAssuranceFromDossierCommand>
{
    public CreateAssuranceFromDossierValidator()
    {
        RuleFor(x => x.DossierId)
            .NotEmpty().WithMessage("L'identifiant du dossier est requis.");
    }
}
