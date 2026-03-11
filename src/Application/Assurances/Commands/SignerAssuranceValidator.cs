using FluentValidation;

namespace AssuranceService.Application.Assurances.Commands;

public class SignerAssuranceValidator : AbstractValidator<SignerAssuranceCommand>
{
    private static readonly HashSet<string> DecisionsValides = new(StringComparer.Ordinal) { "13", "14" };

    public SignerAssuranceValidator()
    {
        RuleFor(x => x.AssuranceId)
            .NotEmpty().WithMessage("L'identifiant de l'assurance est requis.");

        RuleFor(x => x.SignataireId)
            .NotEmpty().WithMessage("L'identifiant du signataire est requis.");

        RuleFor(x => x.Decision)
            .NotEmpty().WithMessage("La décision est requise (13 = Validé, 14 = Modification demandée).")
            .Must(d => DecisionsValides.Contains(d))
            .WithMessage("Décision invalide. Valeurs acceptées : 13 (Validé), 14 (Modification demandée).");
    }
}
