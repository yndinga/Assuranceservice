using FluentValidation;

namespace AssuranceService.Application.Assurances.Commands;

public class SignerAssuranceValidator : AbstractValidator<SignerAssuranceCommand>
{
    private static readonly HashSet<string> DecisionsValides = new(StringComparer.OrdinalIgnoreCase) { "13", "14", "50", "66", "VALIDER", "MODIFICATION_DEMANDEE" };

    public SignerAssuranceValidator()
    {
        RuleFor(x => x.AssuranceId)
            .NotEmpty().WithMessage("L'identifiant de l'assurance est requis.");

        RuleFor(x => x.TypePartenaire)
            .NotEmpty().WithMessage("Le type partenaire est requis.")
            .MaximumLength(50).WithMessage("Le type partenaire ne doit pas depasser 50 caracteres.");

        RuleFor(x => x.Organisation)
            .NotEmpty().WithMessage("Le code organisation du signataire est requis.")
            .MaximumLength(250).WithMessage("Le code organisation ne doit pas depasser 250 caracteres.");

        RuleFor(x => x.Decision)
            .NotEmpty().WithMessage("La decision est requise (50/13 = Valide, 66/14 = Modification demandee).")
            .Must(d => DecisionsValides.Contains(d))
            .WithMessage("Decision invalide. Valeurs acceptees : 50/13 (Valide), 66/14 (Modification demandee).");
    }
}

