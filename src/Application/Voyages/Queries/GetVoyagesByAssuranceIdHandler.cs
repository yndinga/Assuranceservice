using AssuranceService.Application.Common;
using AssuranceService.Application.DTOs;
using MediatR;

namespace AssuranceService.Application.Voyages.Queries;

public class GetVoyagesByAssuranceIdHandler : IRequestHandler<GetVoyagesByAssuranceIdQuery, IEnumerable<VoyageDto>>
{
    private readonly IVoyageRepository _voyageRepository;

    public GetVoyagesByAssuranceIdHandler(IVoyageRepository voyageRepository)
    {
        _voyageRepository = voyageRepository;
    }

    public async Task<IEnumerable<VoyageDto>> Handle(GetVoyagesByAssuranceIdQuery request, CancellationToken cancellationToken)
    {
        var voyages = await _voyageRepository.GetByAssuranceIdAsync(request.AssuranceId);
        return voyages.Select(v => v.ToDto()).ToList();
    }
}



