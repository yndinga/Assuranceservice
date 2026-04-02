using AssuranceService.Application.Avenants.DTOs;
using AssuranceService.Application.Common;
using MediatR;

namespace AssuranceService.Application.Avenants.Queries;

public class GetHistoriquesByAssuranceIdHandler : IRequestHandler<GetHistoriquesByAssuranceIdQuery, IReadOnlyList<HistoriqueLigneListeDto>>
{
    private readonly IAvenantRepository _avenantRepository;

    public GetHistoriquesByAssuranceIdHandler(IAvenantRepository avenantRepository)
    {
        _avenantRepository = avenantRepository;
    }

    public async Task<IReadOnlyList<HistoriqueLigneListeDto>> Handle(GetHistoriquesByAssuranceIdQuery request, CancellationToken cancellationToken)
    {
        var rows = await _avenantRepository.GetHistoriquesByAssuranceIdAsync(request.AssuranceId, cancellationToken);
        return rows
            .Select(h => new HistoriqueLigneListeDto(
                h.Id,
                h.AvenantId,
                h.Avenant?.NoAvenant ?? string.Empty,
                h.CibleEntite,
                h.ReferenceId,
                h.NomChamp,
                h.ValeurAvant,
                h.ValeurApres,
                h.CreerLe))
            .ToList();
    }
}
