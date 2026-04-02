using AssuranceService.Application.Avenants.DTOs;
using AssuranceService.Application.Common;
using AssuranceService.Domain.Models;
using MediatR;

namespace AssuranceService.Application.Avenants.Queries;

public class GetAvenantByIdHandler : IRequestHandler<GetAvenantByIdQuery, AvenantDetailDto?>
{
    private readonly IAvenantRepository _avenantRepository;

    public GetAvenantByIdHandler(IAvenantRepository avenantRepository)
    {
        _avenantRepository = avenantRepository;
    }

    public async Task<AvenantDetailDto?> Handle(GetAvenantByIdQuery request, CancellationToken cancellationToken)
    {
        var a = await _avenantRepository.GetWithHistoriquesAsync(request.AssuranceId, request.AvenantId, cancellationToken);
        if (a == null) return null;

        var historiques = (a.Historiques ?? Array.Empty<Historique>())
            .OrderBy(h => h.NomChamp)
            .Select(h => new HistoriqueDto(
                h.Id,
                h.AvenantId,
                h.CibleEntite,
                h.ReferenceId,
                h.NomChamp,
                h.ValeurAvant,
                h.ValeurApres,
                h.Commentaire,
                h.CreerLe))
            .ToList();

        return new AvenantDetailDto(
            a.Id,
            a.AssuranceId,
            a.NoPolice,
            a.NoAvenant,
            a.Statut,
            a.Motif,
            a.CreerLe,
            historiques);
    }
}
