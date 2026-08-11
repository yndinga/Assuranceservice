using MediatR;

namespace AssuranceService.Application.Assurances.Commands;

/// <summary>
/// Signature d'une assurance par l'organisation habilitee.
/// Prerequis : assurance en Visa demande (11) ou Modification soumise (12).
/// Decision : Valide (13) ou Modification demandee (14).
/// </summary>
public record SignerAssuranceCommand : IRequest<Unit>
{
    public Guid AssuranceId { get; init; }
    public string TypePartenaire { get; init; } = string.Empty;
    public string Organisation { get; init; } = string.Empty;
    public string Decision { get; init; } = "13";
    public string? VisaContent { get; init; }
}

/// <summary>Corps de la requete POST /assurances/{id}/signer.</summary>
public record SignerAssuranceRequest(string TypePartenaire, string Organisation, string Decision, string? VisaContent);
