using AssuranceService.Application.Common;
using AssuranceService.Domain.Constants;
using AssuranceService.Domain.Models;
using MediatR;

namespace AssuranceService.Application.Assurances.Commands;

/// <summary>
/// L'intermédiaire choisit l'assureur pour une demande reçue. AssureurId est renseigné ici ;
/// l'assureur pourra accepter ou refuser plus tard (signature).
/// </summary>
public class ChoisirAssureurHandler : IRequestHandler<ChoisirAssureurCommand, Unit>
{
    private readonly IAssuranceRepository _assuranceRepository;

    public ChoisirAssureurHandler(IAssuranceRepository assuranceRepository)
    {
        _assuranceRepository = assuranceRepository;
    }

    public async Task<Unit> Handle(ChoisirAssureurCommand request, CancellationToken cancellationToken)
    {
        var assurance = await _assuranceRepository.GetByIdAsync(request.AssuranceId);
        if (assurance == null)
            throw new InvalidOperationException($"Assurance {request.AssuranceId} introuvable.");

        if (!assurance.IntermediaireId.HasValue)
            throw new InvalidOperationException("Cette assurance n'a pas été envoyée à un intermédiaire ; le choix d'assureur ne s'applique pas.");

        if (assurance.AssureurId.HasValue)
            throw new InvalidOperationException("Un assureur a déjà été choisi pour cette demande.");

        if (assurance.Statut != StatutAssuranceCodes.Elaboré)
            throw new InvalidOperationException($"Seules les demandes au statut Elaboré (10) peuvent recevoir un choix d'assureur. Statut actuel : {assurance.Statut}.");

        assurance.AssureurId = request.AssureurId;
        assurance.ModifierLe = DateTime.UtcNow;
        await _assuranceRepository.UpdateAsync(assurance);

        return Unit.Value;
    }
}
