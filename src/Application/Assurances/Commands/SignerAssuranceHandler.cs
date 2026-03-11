using AssuranceService.Application.Common;
using AssuranceService.Domain.Constants;
using AssuranceService.Domain.Models;
using MediatR;

namespace AssuranceService.Application.Assurances.Commands;

/// <summary>
/// Traite la signature : vérifie que le signataire est bien l'assureur ou l'intermédiaire,
/// que l'assurance est en Visa demandé ou Modification soumise, puis enregistre le visa et met à jour le statut.
/// À venir : le backend détectera l'ID du signataire (connecté), fera une requête pour récupérer le token
/// contenant NoPolice, Organisation, Partenaire, VisaOK, PartenaireId, Timestamp, nom du signataire, et infos du certificat
/// (Le certificat appartient à : Nom, Organisation, Pays, Valide), puis renseignera VisaContent.
/// </summary>
public class SignerAssuranceHandler : IRequestHandler<SignerAssuranceCommand, Unit>
{
    private readonly IAssuranceRepository _assuranceRepository;

    public SignerAssuranceHandler(IAssuranceRepository assuranceRepository)
    {
        _assuranceRepository = assuranceRepository;
    }

    public async Task<Unit> Handle(SignerAssuranceCommand request, CancellationToken cancellationToken)
    {
        var assurance = await _assuranceRepository.GetByIdAsync(request.AssuranceId);
        if (assurance == null)
            throw new InvalidOperationException($"Assurance {request.AssuranceId} introuvable.");

        // Statut autorisé : Visa demandé (11) ou Modification soumise (12)
        if (assurance.Statut != StatutAssuranceCodes.VisaDemandé && assurance.Statut != StatutAssuranceCodes.ModificationSoumise)
            throw new InvalidOperationException(
                "La signature n'est possible que pour une assurance en « Visa demandé » (11) ou « Modification soumise » (12). Statut actuel : " + assurance.Statut + ".");

        // Signataire doit être l'assureur ou l'intermédiaire
        var estAssureur = assurance.AssureurId == request.SignataireId;
        var estIntermediaire = assurance.IntermediaireId == request.SignataireId;
        if (!estAssureur && !estIntermediaire)
            throw new InvalidOperationException("Seul l'assureur ou l'intermédiaire désigné sur cette assurance peut la signer.");

        // Créer le visa (VisaContent sera rempli plus tard via token : NoPolice, Organisation, Partenaire, PartenaireId, Timestamp, nom du signataire, certificat Nom/Organisation/Pays/Valide)
        var visa = new VisaAssurance
        {
            Id = Guid.NewGuid(),
            AssuranceId = assurance.Id,
            OrganisationId = request.SignataireId,
            VisaOK = request.Decision == StatutAssuranceCodes.Validé,
            VisaContent = request.VisaContent, // À venir : récupéré depuis le token (backend appelle le service avec l'ID du signataire)
            CreerPar = "System",
            ModifierPar = "System",
            CreerLe = DateTime.UtcNow,
            ModifierLe = DateTime.UtcNow
        };

        assurance.Statut = request.Decision;
        assurance.ModifierLe = DateTime.UtcNow;

        await _assuranceRepository.AddVisaAssuranceAsync(visa);
        await _assuranceRepository.UpdateAsync(assurance);

        return Unit.Value;
    }
}
