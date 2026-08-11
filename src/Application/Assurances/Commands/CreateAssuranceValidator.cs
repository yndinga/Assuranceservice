using AssuranceService.Domain.Constants;
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
            .Must(v => int.TryParse(v?.Trim(), out var days) && days > 0)
            .WithMessage("La durée en jours doit être un entier strictement positif.");

        RuleFor(x => x.ModeDeTransport)
            .NotEmpty().WithMessage("Le mode de transport est requis (ex: MA, AR, RO, FL).")
            .MaximumLength(10);

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
        RuleFor(x => x.NomNavire)
            .NotEmpty().When(x => IsMaritimeOrFluvial(x.ModeDeTransport))
            .WithMessage("NomNavire requis pour MA et FL.")
            .MaximumLength(255);
        RuleFor(x => x.TypeNavire)
            .NotEmpty().When(x => IsMaritimeOrFluvial(x.ModeDeTransport))
            .WithMessage("TypeNavire requis pour MA et FL.")
            .MaximumLength(100);
        RuleFor(x => x.LieuSejour).MaximumLength(255).When(x => !string.IsNullOrWhiteSpace(x.LieuSejour));
        RuleFor(x => x.DureeSejour).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.DureeSejour));
        RuleFor(x => x.PaysProvenance).MaximumLength(255).When(x => !string.IsNullOrWhiteSpace(x.PaysProvenance));
        RuleFor(x => x.PaysDestination).MaximumLength(255).When(x => !string.IsNullOrWhiteSpace(x.PaysDestination));

        RuleFor(x => x.PortEmbarquement)
            .NotEmpty().When(x => string.Equals(x.ModeDeTransport, "MA", StringComparison.OrdinalIgnoreCase))
            .WithMessage("PortEmbarquement requis pour MA.");
        RuleFor(x => x.PortDebarquement)
            .NotEmpty().When(x => string.Equals(x.ModeDeTransport, "MA", StringComparison.OrdinalIgnoreCase))
            .WithMessage("PortDebarquement requis pour MA.");
        RuleFor(x => x.AeroportEmbarquement)
            .NotEmpty().When(x => string.Equals(x.ModeDeTransport, "AR", StringComparison.OrdinalIgnoreCase)
                || string.Equals(x.ModeDeTransport, "AE", StringComparison.OrdinalIgnoreCase))
            .WithMessage("AeroportEmbarquement requis pour AR.");
        RuleFor(x => x.RouteNationale)
            .NotEmpty().When(x => string.Equals(x.ModeDeTransport, "RO", StringComparison.OrdinalIgnoreCase))
            .WithMessage("RouteNationale requise pour RO.");
        RuleFor(x => x.PortEmbarquement)
            .NotEmpty().When(x => string.Equals(x.ModeDeTransport, "FL", StringComparison.OrdinalIgnoreCase))
            .WithMessage("PortEmbarquement requis pour FL.");

        RuleFor(x => x.NumeroBL)
            .MaximumLength(255)
            .When(x => !string.IsNullOrWhiteSpace(x.NumeroBL));
        RuleFor(x => x.NumeroLTA)
            .MaximumLength(255)
            .When(x => !string.IsNullOrWhiteSpace(x.NumeroLTA));
        RuleFor(x => x.NumeroLV)
            .MaximumLength(255)
            .When(x => !string.IsNullOrWhiteSpace(x.NumeroLV));

        RuleFor(x => x.Garantie)
            .MaximumLength(250)
            .When(x => !string.IsNullOrWhiteSpace(x.Garantie));

        RuleFor(x => x)
            .Must(x => !x.DateFin.HasValue || !x.DateDebut.HasValue || x.DateFin >= x.DateDebut)
            .WithMessage("La date de fin doit être supérieure ou égale à la date de début.");
    }

    private static bool IsMaritimeOrFluvial(string? mode)
    {
        var normalized = ModeDeTransportCodes.Normalize(mode);
        return normalized is ModeDeTransportCodes.Maritime or ModeDeTransportCodes.Fluvial;
    }
}
