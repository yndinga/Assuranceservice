using MediatR;

namespace AssuranceService.Application.Assurances.Commands;

/// <summary>
/// Action distincte : l'intermédiaire (courtier ou agent général) choisit la maison d'assurance pour une demande qui lui a été envoyée.
/// L'assureur pourra ensuite accepter ou refuser à l'étape signature (il est engagé).
/// Prérequis : l'assurance a IntermediaireId renseigné et AssureurId vide.
/// </summary>
public record ChoisirAssureurCommand : IRequest<Unit>
{
    public Guid AssuranceId { get; init; }
    /// <summary>Id du partenaire (maison d'assurance) choisi par l'intermédiaire.</summary>
    public Guid AssureurId { get; init; }
}

/// <summary>Corps de la requête POST /assurances/{id}/choisir-assureur.</summary>
public record ChoisirAssureurRequest(Guid AssureurId);
