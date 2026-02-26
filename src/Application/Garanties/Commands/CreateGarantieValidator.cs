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
            .MaximumLength(25).When(x => !string.IsNullOrWhiteSpace(x.Taux));

        RuleFor(x => x.Accessoires)
            .GreaterThanOrEqualTo(0).WithMessage("Les accessoires doivent être positifs ou nuls.");
    }
}
