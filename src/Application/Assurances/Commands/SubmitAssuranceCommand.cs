using MediatR;

namespace AssuranceService.Application.Assurances.Commands;

/// <summary>
/// Commande pour soumettre une assurance (Étape 4).
/// Prérequis : l'assurance doit avoir au moins un document et une prime.
/// AssureurId doit déjà être renseigné (saisi à la création ou via ChoisirAssureur par l'intermédiaire).
/// - Génère NoPolice et NumeroCert
/// - Change le statut à 11 (Visa demandé)
/// </summary>
public record SubmitAssuranceCommand : IRequest<SubmitAssuranceResponse>
{
    public Guid AssuranceId { get; init; }
    // GarantieId lu depuis Assurance.GarantieId (sauvegardé à la création)
    // AssureurId lu depuis l'assurance (création directe ou ChoisirAssureur).
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
    public string Statut { get; init; } = "10";
}



