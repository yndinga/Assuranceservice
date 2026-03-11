using AssuranceService.Application.Common;
using AssuranceService.Application.DTOs;
using MediatR;

namespace AssuranceService.Application.Assurances.Queries;

public class GetAssurancesByAssureurHandler : IRequestHandler<GetAssurancesByAssureurQuery, IEnumerable<AssuranceDto>>
{
    private readonly IAssuranceRepository _assuranceRepository;

    public GetAssurancesByAssureurHandler(IAssuranceRepository assuranceRepository)
    {
        _assuranceRepository = assuranceRepository;
    }

    public async Task<IEnumerable<AssuranceDto>> Handle(GetAssurancesByAssureurQuery request, CancellationToken cancellationToken)
    {
        var assurances = await _assuranceRepository.GetByAssureurIdAsync(request.AssureurId);
        return assurances.ToDtoList();
    }
}
