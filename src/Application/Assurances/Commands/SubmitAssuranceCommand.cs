using MediatR;

namespace AssuranceService.Application.Assurances.Commands;

/// <summary>
/// Commande pour soumettre une assurance (Étape 4)
/// - Génère NoPolice et NumeroCert
/// - Calcule la prime
/// - Change le statut à "VisaDemandé"
/// </summary>
public record SubmitAssuranceCommand : IRequest<SubmitAssuranceResponse>
{
    public Guid AssuranceId { get; init; }
    // GarantieId lu depuis Assurance.GarantieId (sauvegardé à la création)
    // Devise lue automatiquement depuis les marchandises (Marchandise.Devise)
    // Requis quand l'assurance a été envoyée à un intermédiaire (il a choisi l'assureur)
    public Guid? PartenaireId { get; init; }
}

public record SubmitAssuranceResponse
{
    public Guid AssuranceId { get; init; }
    public string NoPolice { get; init; } = string.Empty;
    public string NumeroCert { get; init; } = string.Empty;
    public decimal ValeurFCFA { get; init; }
    public decimal PrimeNette { get; init; }
    public decimal Accessoires { get; init; }
    public decimal Taxe { get; init; }
    public decimal PrimeTotale { get; init; }
    public string Statut { get; init; } = string.Empty;
}



