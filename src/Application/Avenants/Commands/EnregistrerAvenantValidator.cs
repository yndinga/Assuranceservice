using FluentValidation;
using AssuranceService.Domain.Constants;

namespace AssuranceService.Application.Avenants.Commands;

public class EnregistrerAvenantValidator : AbstractValidator<EnregistrerAvenantCommand>
{
    public EnregistrerAvenantValidator()
    {
        RuleFor(x => x.AssuranceId).NotEmpty();
        RuleFor(x => x.Type)
            .NotEmpty()
            .MaximumLength(50)
            .Must(t => AvenantTypes.Allowed.Contains(t.Trim().ToUpperInvariant()))
            .WithMessage($"Type d'avenant invalide. Valeurs autorisées : {AvenantTypes.Modification}, {AvenantTypes.Prorogation}, {AvenantTypes.Annulation}.");
        RuleFor(x => x.Motif).NotEmpty().MaximumLength(8000);
    }
}
