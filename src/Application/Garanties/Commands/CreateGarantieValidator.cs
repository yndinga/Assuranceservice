using FluentValidation;

namespace AssuranceService.Application.Garanties.Commands;

public class CreateGarantieValidator : AbstractValidator<CreateGarantieCommand>
{
    public CreateGarantieValidator()
    {
        RuleFor(x => x.NomGarantie)
            .NotEmpty().WithMessage("Le nom de la garantie est requis.")
            .MaximumLength(255);

        RuleFor(x => x.Taux)
            .InclusiveBetween(0m, 1m).When(x => x.Taux.HasValue)
            .WithMessage("Le taux doit être entre 0 et 1 (ex. 0,002).");

        RuleFor(x => x.Accessoires)
            .GreaterThanOrEqualTo(0).WithMessage("Les accessoires doivent être positifs ou nuls.");
    }
}
