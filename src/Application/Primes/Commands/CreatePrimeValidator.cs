using FluentValidation;

namespace AssuranceService.Application.Primes.Commands;

public class CreatePrimeValidator : AbstractValidator<CreatePrimeCommand>
{
    public CreatePrimeValidator()
    {
        RuleFor(x => x.AssuranceId)
            .NotEmpty().WithMessage("L'identifiant de l'assurance est requis.");

        RuleFor(x => x.ValeurFCFA)
            .GreaterThanOrEqualTo(0).WithMessage("La valeur en FCFA doit être positive ou nulle.");

        RuleFor(x => x.ValeurDevise)
            .GreaterThanOrEqualTo(0).WithMessage("La valeur en devise doit être positive ou nulle.");

        RuleFor(x => x.Statut)
            .NotEmpty().WithMessage("Le statut est requis.")
            .MaximumLength(255);

        RuleFor(x => x.Taux)
            .MaximumLength(255).When(x => !string.IsNullOrWhiteSpace(x.Taux));

        RuleFor(x => x.PrimeNette)
            .GreaterThanOrEqualTo(0).When(x => x.PrimeNette.HasValue);

        RuleFor(x => x.Accessoires)
            .GreaterThanOrEqualTo(0).When(x => x.Accessoires.HasValue);

        RuleFor(x => x.Taxe)
            .GreaterThanOrEqualTo(0).When(x => x.Taxe.HasValue);

        RuleFor(x => x.PrimeTotale)
            .GreaterThanOrEqualTo(0).When(x => x.PrimeTotale.HasValue);
    }
}
