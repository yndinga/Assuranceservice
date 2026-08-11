using MediatR;

namespace AssuranceService.Application.Assurances.Commands;

/// <summary>
/// L'intermediaire choisit la maison d'assurance pour une demande recue.
/// </summary>
public record ChoisirAssureurCommand : IRequest<Unit>
{
    public Guid AssuranceId { get; init; }
    public string Assureur { get; init; } = string.Empty;
}

/// <summary>Corps de la requete POST /assurances/{id}/choisir-assureur.</summary>
public record ChoisirAssureurRequest(string Assureur);
