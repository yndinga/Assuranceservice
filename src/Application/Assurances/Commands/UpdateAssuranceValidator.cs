using AssuranceService.Domain.Constants;
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

        RuleFor(x => x.ModeDeTransport)
            .NotEmpty().WithMessage("Le mode de transport est requis.")
            .MaximumLength(10);

        RuleFor(x => x.NoPolice)
            .MaximumLength(250).When(x => !string.IsNullOrWhiteSpace(x.NoPolice));

        RuleFor(x => x.NumeroCert)
            .MaximumLength(250).When(x => !string.IsNullOrWhiteSpace(x.NumeroCert));

        RuleFor(x => x.Duree)
            .Must(v => int.TryParse(v?.Trim(), out var days) && days > 0)
            .WithMessage("La durée en jours doit être un entier strictement positif.");

        RuleFor(x => x.Garantie)
            .MaximumLength(250).When(x => !string.IsNullOrWhiteSpace(x.Garantie));

        RuleFor(x => x.NomTransporteur)
            .MaximumLength(250).When(x => !string.IsNullOrWhiteSpace(x.NomTransporteur));

        RuleFor(x => x.Transit)
            .MaximumLength(350).When(x => !string.IsNullOrWhiteSpace(x.Transit));

        RuleFor(x => x.NomNavire)
            .NotEmpty()
            .When(x => IsMaritimeOrFluvial(x.ModeDeTransport))
            .WithMessage("NomNavire requis pour MA et FL.")
            .MaximumLength(255);

        RuleFor(x => x.TypeNavire)
            .NotEmpty()
            .When(x => IsMaritimeOrFluvial(x.ModeDeTransport))
            .WithMessage("TypeNavire requis pour MA et FL.")
            .MaximumLength(100);

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
