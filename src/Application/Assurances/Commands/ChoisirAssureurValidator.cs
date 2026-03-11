using FluentValidation;

namespace AssuranceService.Application.Assurances.Commands;

public class ChoisirAssureurValidator : AbstractValidator<ChoisirAssureurCommand>
{
    public ChoisirAssureurValidator()
    {
        RuleFor(x => x.AssuranceId)
            .NotEmpty().WithMessage("L'identifiant de l'assurance est requis.");

        RuleFor(x => x.AssureurId)
            .NotEmpty().WithMessage("L'identifiant de l'assureur (maison d'assurance) choisi est requis.");
    }
}
