using FluentValidation;

namespace AssuranceService.Application.Assurances.Commands;

public class CreateAssuranceValidator : AbstractValidator<CreateAssuranceCommand>
{
    private const string PlaceholderString = "string";

    public CreateAssuranceValidator()
    {
        RuleFor(x => x.ImportateurNom)
            .NotEmpty().WithMessage("Le nom de l'importateur est requis.")
            .Must(v => !string.Equals(v, PlaceholderString, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Veuillez fournir une valeur réelle, le placeholder 'string' n'est pas accepté.")
            .MaximumLength(250);

        RuleFor(x => x.ImportateurNIU)
            .Must(v => string.IsNullOrWhiteSpace(v) || !string.Equals(v, PlaceholderString, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Le placeholder 'string' n'est pas accepté.")
            .MaximumLength(25).When(x => !string.IsNullOrWhiteSpace(x.ImportateurNIU));

        RuleFor(x => x.TypeContrat)
            .NotEmpty().WithMessage("Le type de contrat est requis.")
            .Must(v => !string.Equals(v, PlaceholderString, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Veuillez fournir une valeur réelle, le placeholder 'string' n'est pas accepté.")
            .MaximumLength(250);

        RuleFor(x => x.Duree)
            .Must(v => string.IsNullOrWhiteSpace(v) || !string.Equals(v, PlaceholderString, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Le placeholder 'string' n'est pas accepté.")
            .MaximumLength(250).When(x => !string.IsNullOrWhiteSpace(x.Duree));

        RuleFor(x => x.Module)
            .NotEmpty().WithMessage("Le module est requis (ex: MA, AE, RO, FL).")
            .MaximumLength(250);

        RuleFor(x => x.GarantieId)
            .NotEmpty().WithMessage("La garantie (GarantieId) est requise pour le calcul de la prime.")
            .When(x => x.GarantieId.HasValue);

        RuleFor(x => x)
            .Must(x => !x.DateFin.HasValue || !x.DateDebut.HasValue || x.DateFin >= x.DateDebut)
            .WithMessage("La date de fin doit être supérieure ou égale à la date de début.");
    }
}
