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

        RuleFor(x => x.Designation).MaximumLength(255).When(x => !string.IsNullOrWhiteSpace(x.Designation));
        RuleFor(x => x.Nature).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Nature));
        RuleFor(x => x.Specificites).MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.Specificites));
        RuleFor(x => x.Conditionnement).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Conditionnement));
        RuleFor(x => x.Description).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Description));
        RuleFor(x => x.Devise).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.Devise));
        RuleFor(x => x.MasseBrute).MaximumLength(255).When(x => !string.IsNullOrWhiteSpace(x.MasseBrute));
        RuleFor(x => x.UniteStatistique).MaximumLength(255).When(x => !string.IsNullOrWhiteSpace(x.UniteStatistique));
        RuleFor(x => x.Marque).MaximumLength(255).When(x => !string.IsNullOrWhiteSpace(x.Marque));
        RuleFor(x => x.NomTransporteur).MaximumLength(255).When(x => !string.IsNullOrWhiteSpace(x.NomTransporteur));
        RuleFor(x => x.NomNavire).MaximumLength(255).When(x => !string.IsNullOrWhiteSpace(x.NomNavire));
        RuleFor(x => x.TypeNavire).MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.TypeNavire));
        RuleFor(x => x.LieuSejour).MaximumLength(255).When(x => !string.IsNullOrWhiteSpace(x.LieuSejour));
        RuleFor(x => x.DureeSejour).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.DureeSejour));
        RuleFor(x => x.PaysProvenance).MaximumLength(255).When(x => !string.IsNullOrWhiteSpace(x.PaysProvenance));
        RuleFor(x => x.PaysDestination).MaximumLength(255).When(x => !string.IsNullOrWhiteSpace(x.PaysDestination));

        RuleFor(x => x.PortEmbarquement)
            .NotEmpty().When(x => string.Equals(x.Module, "MA", StringComparison.OrdinalIgnoreCase))
            .WithMessage("PortEmbarquement requis pour MA.");
        RuleFor(x => x.PortDebarquement)
            .NotEmpty().When(x => string.Equals(x.Module, "MA", StringComparison.OrdinalIgnoreCase))
            .WithMessage("PortDebarquement requis pour MA.");
        RuleFor(x => x.AeroportEmbarquement)
            .NotEmpty().When(x => string.Equals(x.Module, "AE", StringComparison.OrdinalIgnoreCase))
            .WithMessage("AeroportEmbarquement requis pour AE.");
        RuleFor(x => x.RouteNationale)
            .NotEmpty().When(x => string.Equals(x.Module, "RO", StringComparison.OrdinalIgnoreCase))
            .WithMessage("RouteNationale requise pour RO.");
        RuleFor(x => x.PortEmbarquement)
            .NotEmpty().When(x => string.Equals(x.Module, "FL", StringComparison.OrdinalIgnoreCase))
            .WithMessage("PortEmbarquement requis pour FL.");

        RuleFor(x => x.GarantieId)
            .NotEmpty().WithMessage("La garantie (GarantieId) est requise pour le calcul de la prime.")
            .When(x => x.GarantieId.HasValue);

        RuleFor(x => x)
            .Must(x => !x.DateFin.HasValue || !x.DateDebut.HasValue || x.DateFin >= x.DateDebut)
            .WithMessage("La date de fin doit être supérieure ou égale à la date de début.");
    }
}
