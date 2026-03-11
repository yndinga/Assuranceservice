using MediatR;

namespace AssuranceService.Application.Assurances.Commands;

/// <summary>
/// Signature d'une assurance par l'assureur ou l'intermédiaire.
/// Prérequis : assurance en Visa demandé (11) ou Modification soumise (12).
/// Décision : Validé (13) ou Modification demandée (14).
/// </summary>
public record SignerAssuranceCommand : IRequest<Unit>
{
    public Guid AssuranceId { get; init; }
    /// <summary>Id du signataire (doit être AssureurId ou IntermediaireId de l'assurance).</summary>
    public Guid SignataireId { get; init; }
    /// <summary>Code statut après signature : "13" (Validé) ou "14" (Modification demandée).</summary>
    public string Decision { get; init; } = "13";
    /// <summary>
    /// Contenu du visa : renseigné côté serveur après requête token (ID signataire → token avec NoPolice, Organisation, Partenaire, PartenaireId, Timestamp, nom du signataire, certificat : Nom, Organisation, Pays, Valide).
    /// Ne vient pas du formulaire client.
    /// </summary>
    public string? VisaContent { get; init; }
}

/// <summary>Corps de la requête POST /assurances/{id}/signer. VisaContent ne vient pas du formulaire — sera rempli côté serveur (signataire connecté).</summary>
public record SignerAssuranceRequest(Guid SignataireId, string Decision);
