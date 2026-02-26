using FluentValidation;

namespace AssuranceService.Application.Assurances.Commands;

public class UpdateAssuranceValidator : AbstractValidator<UpdateAssuranceCommand>
{
    public UpdateAssuranceValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("L'identifiant de l'assurance est requis.");

        RuleFor(x => x.Importateur)
            .NotEmpty().WithMessage("Le nom de l'importateur est requis.")
            .MaximumLength(250);

        RuleFor(x => x.TypeContrat)
            .NotEmpty().WithMessage("Le type de contrat est requis.")
            .MaximumLength(250);

        RuleFor(x => x.Statut)
            .NotEmpty().WithMessage("Le statut est requis.")
            .MaximumLength(250);

        RuleFor(x => x.NoPolice)
            .MaximumLength(250).When(x => !string.IsNullOrWhiteSpace(x.NoPolice));

        RuleFor(x => x.NumeroCert)
            .MaximumLength(250).When(x => !string.IsNullOrWhiteSpace(x.NumeroCert));

        RuleFor(x => x.Duree)
            .MaximumLength(250).When(x => !string.IsNullOrWhiteSpace(x.Duree));

        RuleFor(x => x.GarantieId)
            .NotEmpty().When(x => x.GarantieId.HasValue);

        RuleFor(x => x.NomTransporteur)
            .MaximumLength(250).When(x => !string.IsNullOrWhiteSpace(x.NomTransporteur));

        RuleFor(x => x.NomNavire)
            .MaximumLength(250).When(x => !string.IsNullOrWhiteSpace(x.NomNavire));

        RuleFor(x => x.TypeNavire)
            .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.TypeNavire));

        RuleFor(x => x)
            .Must(x => !x.DateFin.HasValue || !x.DateDebut.HasValue || x.DateFin >= x.DateDebut)
            .WithMessage("La date de fin doit être supérieure ou égale à la date de début.");
    }
}
