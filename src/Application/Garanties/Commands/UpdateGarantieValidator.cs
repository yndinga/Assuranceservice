using FluentValidation;

namespace AssuranceService.Application.Garanties.Commands;

public class UpdateGarantieValidator : AbstractValidator<UpdateGarantieCommand>
{
    public UpdateGarantieValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("L'identifiant de la garantie est requis.");

        RuleFor(x => x.NomGarantie)
            .NotEmpty().WithMessage("Le nom de la garantie est requis.")
            .MaximumLength(255);

        RuleFor(x => x.Taux)
            .MaximumLength(25).When(x => !string.IsNullOrWhiteSpace(x.Taux));

        RuleFor(x => x.Accessoires)
            .GreaterThanOrEqualTo(0).WithMessage("Les accessoires doivent être positifs ou nuls.");
    }
}
