using FluentValidation;

namespace AssuranceService.Application.Assurances.Commands;

public class ChoisirAssureurValidator : AbstractValidator<ChoisirAssureurCommand>
{
    public ChoisirAssureurValidator()
    {
        RuleFor(x => x.AssuranceId)
            .NotEmpty().WithMessage("L'identifiant de l'assurance est requis.");

        RuleFor(x => x.Assureur)
            .NotEmpty().WithMessage("Le code assureur choisi est requis.")
            .MaximumLength(250).WithMessage("Le code assureur ne doit pas depasser 250 caracteres.");
    }
}
