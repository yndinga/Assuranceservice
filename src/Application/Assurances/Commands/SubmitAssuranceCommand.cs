using MediatR;

namespace AssuranceService.Application.Assurances.Commands;

/// <summary>
/// Commande pour soumettre une assurance.
/// Prerequis : document, prime, assureur dans Assurances.Partenaire,
/// et signataire cree dans VisaAssurances lors de la soumission.
/// </summary>
public record SubmitAssuranceCommand : IRequest<SubmitAssuranceResponse>
{
    public Guid AssuranceId { get; init; }
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
    public string Statut { get; init; } = "42";
}

