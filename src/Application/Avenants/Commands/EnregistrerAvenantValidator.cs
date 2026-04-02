using FluentValidation;

namespace AssuranceService.Application.Avenants.Commands;

public class EnregistrerAvenantValidator : AbstractValidator<EnregistrerAvenantCommand>
{
    public EnregistrerAvenantValidator()
    {
        RuleFor(x => x.AssuranceId).NotEmpty();
        RuleFor(x => x.Motif).NotEmpty().MaximumLength(8000);
    }
}
