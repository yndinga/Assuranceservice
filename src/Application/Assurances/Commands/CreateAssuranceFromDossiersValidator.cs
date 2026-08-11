using FluentValidation;

namespace AssuranceService.Application.Assurances.Commands;

public sealed class CreateAssuranceFromDossiersValidator : AbstractValidator<CreateAssuranceFromDossiersCommand>
{
    public CreateAssuranceFromDossiersValidator()
    {
        RuleFor(command => command.Duree)
            .Must(BePositiveDuration)
            .WithMessage("La durée en jours doit être un entier strictement positif.");

        RuleFor(command => command.DossierIds)
            .NotEmpty().WithMessage("Au moins une DI est requise.")
            .Must(ids => ids.All(id => id != Guid.Empty)).WithMessage("Les identifiants DI doivent être valides.")
            .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Une DI ne peut être sélectionnée qu'une fois.");

        RuleFor(command => command.Lignes)
            .NotEmpty().WithMessage("Au moins une ligne DI est requise.")
            .Must(lines => lines.All(line => line.DossierId != Guid.Empty && line.LigneDIId != Guid.Empty))
            .WithMessage("Chaque ligne doit référencer une DI et une ligne DI valides.")
            .Must(lines => lines.Select(line => line.LigneDIId).Distinct().Count() == lines.Count)
            .WithMessage("Une ligne DI ne peut être sélectionnée qu'une fois.");

        RuleForEach(command => command.Lignes).ChildRules(line =>
        {
            line.RuleFor(item => item)
                .Must(item => Values(item).All(value => value is null || value > 0m))
                .WithMessage("Toute valeur assurée renseignée doit être strictement positive.");
        });
    }

    private static IEnumerable<decimal?> Values(AssuranceLigneSelection line)
    {
        yield return line.Quantite;
        yield return line.MasseBrute;
        yield return line.MasseNette;
        yield return line.Volume;
        yield return line.ValeurDevise;
        yield return line.ValeurXAF;
    }

    private static bool BePositiveDuration(string? value) =>
        int.TryParse(value?.Trim(), out var days) && days > 0;
}
