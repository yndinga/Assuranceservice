using AssuranceService.Application.Common;
using AssuranceService.Application.DTOs;
using MediatR;

namespace AssuranceService.Application.Assurances.Queries;

public class GetAssurancesByIntermediaireHandler : IRequestHandler<GetAssurancesByIntermediaireQuery, IEnumerable<AssuranceDto>>
{
    private readonly IAssuranceRepository _assuranceRepository;

    public GetAssurancesByIntermediaireHandler(IAssuranceRepository assuranceRepository)
    {
        _assuranceRepository = assuranceRepository;
    }

    public async Task<IEnumerable<AssuranceDto>> Handle(GetAssurancesByIntermediaireQuery request, CancellationToken cancellationToken)
    {
        var assurances = await _assuranceRepository.GetByIntermediaireIdAsync(request.IntermediaireId);
        return assurances.ToDtoList();
    }
}
