using AssuranceService.Application.Avenants.DTOs;
using AssuranceService.Application.Common;
using MediatR;

namespace AssuranceService.Application.Avenants.Queries;

public class GetAvenantsByAssuranceIdHandler : IRequestHandler<GetAvenantsByAssuranceIdQuery, IReadOnlyList<AvenantListItemDto>>
{
    private readonly IAvenantRepository _avenantRepository;

    public GetAvenantsByAssuranceIdHandler(IAvenantRepository avenantRepository)
    {
        _avenantRepository = avenantRepository;
    }

    public async Task<IReadOnlyList<AvenantListItemDto>> Handle(GetAvenantsByAssuranceIdQuery request, CancellationToken cancellationToken)
    {
        var list = await _avenantRepository.ListByAssuranceIdAsync(request.AssuranceId, cancellationToken);
        return list
            .OrderByDescending(a => a.CreerLe)
            .Select(a => new AvenantListItemDto(
                a.Id,
                a.NoPolice,
                a.NoAvenant,
                a.Type,
                a.Statut,
                a.Motif,
                a.CreerLe,
                a.Historiques?.Count ?? 0))
            .ToList();
    }
}
