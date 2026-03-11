using FluentValidation;

namespace AssuranceService.Application.Voyages.Commands;

public class CreateVoyageValidator : AbstractValidator<CreateVoyageCommand>
{
    private const string Placeholder = "string";

    private static readonly HashSet<string> _codesValides =
        new(StringComparer.OrdinalIgnoreCase) { "MA", "AE", "RO", "FL" };

    public CreateVoyageValidator()
    {
        // ── Identifiant assurance ───────────────────────────────────────────
        RuleFor(x => x.AssuranceId)
            .NotEmpty().WithMessage("L'identifiant de l'assurance est requis.");

        // ── Module (type de transport) : code MA, AE, RO, FL ─────────────────
        RuleFor(x => x.ModuleCode)
            .NotEmpty().WithMessage("Le code module est requis (MA, AE, RO, FL).")
            .Must(code => _codesValides.Contains(code?.Trim() ?? string.Empty))
            .WithMessage("Module invalide. Valeurs acceptées : MA (Maritime), AE (Aérien), RO (Routier), FL (Fluvial).");

        // ── Transporteur ────────────────────────────────────────────────────
        RuleFor(x => x.NomTransporteur)
            .NotEmpty().WithMessage("Le nom du transporteur est requis.")
            .Must(v => !IsPlaceholder(v)).WithMessage("Veuillez fournir un nom de transporteur réel.")
            .MaximumLength(250);

        RuleFor(x => x.NomNavire)
            .Must(v => !IsPlaceholder(v)).WithMessage("Veuillez fournir un nom de navire réel.")
            .MaximumLength(250)
            .When(x => !string.IsNullOrWhiteSpace(x.NomNavire));

        RuleFor(x => x.TypeNavire)
            .Must(v => !IsPlaceholder(v)).WithMessage("Veuillez fournir un type de navire réel.")
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.TypeNavire));

        // ── Pays ─────────────────────────────────────────────────────────────
        RuleFor(x => x.PaysProvenance)
            .NotEmpty().WithMessage("Le pays de provenance est requis.")
            .Must(v => !IsPlaceholder(v)).WithMessage("Veuillez fournir un pays de provenance réel.")
            .MaximumLength(250);

        RuleFor(x => x.PaysDestination)
            .NotEmpty().WithMessage("Le pays de destination est requis.")
            .Must(v => !IsPlaceholder(v)).WithMessage("Veuillez fournir un pays de destination réel.")
            .MaximumLength(250);

        // ── Séjour : défini par l'importateur, optionnel (peut être null) ──────
        RuleFor(x => x.LieuSejour)
            .Must(v => !IsPlaceholder(v)).WithMessage("Veuillez fournir un lieu de séjour réel.")
            .MaximumLength(255)
            .When(x => !string.IsNullOrWhiteSpace(x.LieuSejour));

        RuleFor(x => x.DureeSejour)
            .Must(v => !IsPlaceholder(v)).WithMessage("Veuillez fournir une durée de séjour réelle.")
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.DureeSejour));

        // ── Maritime (MA) : ports (référentiel Ports) requis ─────────────────
        RuleFor(x => x.PortEmbarquementId)
            .NotEmpty().WithMessage("Le port d'embarquement (PortEmbarquementId) est requis pour un transport Maritime.")
            .When(x => IsModuleCode(x.ModuleCode, "MA"));

        RuleFor(x => x.PortDebarquementId)
            .NotEmpty().WithMessage("Le port de débarquement (PortDebarquementId) est requis pour un transport Maritime.")
            .When(x => IsModuleCode(x.ModuleCode, "MA"));

        // ── Aérien (AE) : aéroports requis ──────────────────────────────────
        RuleFor(x => x.AeroportEmbarquement)
            .NotEmpty().WithMessage("L'aéroport d'embarquement est requis pour un transport Aérien.")
            .Must(v => !IsPlaceholder(v)).WithMessage("Veuillez fournir un aéroport d'embarquement réel.")
            .MaximumLength(255)
            .When(x => IsModuleCode(x.ModuleCode, "AE"));

        RuleFor(x => x.AeroportDebarquement)
            .NotEmpty().WithMessage("L'aéroport de débarquement est requis pour un transport Aérien.")
            .Must(v => !IsPlaceholder(v)).WithMessage("Veuillez fournir un aéroport de débarquement réel.")
            .MaximumLength(255)
            .When(x => IsModuleCode(x.ModuleCode, "AE"));

        // ── Routier (RO) : route nationale requise ───────────────────────────
        RuleFor(x => x.RouteNationale)
            .NotEmpty().WithMessage("La route nationale est requise pour un transport Routier.")
            .Must(v => !IsPlaceholder(v)).WithMessage("Veuillez fournir une route nationale réelle.")
            .MaximumLength(255)
            .When(x => IsModuleCode(x.ModuleCode, "RO"));

        // ── Fluvial (FL) : port d'embarquement requis (référentiel Ports) ────
        RuleFor(x => x.PortEmbarquementId)
            .NotEmpty().WithMessage("Le port d'embarquement (PortEmbarquementId) est requis pour un transport Fluvial.")
            .When(x => IsModuleCode(x.ModuleCode, "FL"));
    }

    private static bool IsModuleCode(string? moduleCode, string code) =>
        string.Equals(moduleCode?.Trim(), code, StringComparison.OrdinalIgnoreCase);

    private static bool IsPlaceholder(string? v) =>
        string.Equals(v, Placeholder, StringComparison.OrdinalIgnoreCase);
}
