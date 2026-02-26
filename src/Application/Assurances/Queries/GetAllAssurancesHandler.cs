using AssuranceService.Application.Common;
using AssuranceService.Application.DTOs;
using MediatR;

namespace AssuranceService.Application.Assurances.Queries;

public class GetAllAssurancesHandler : IRequestHandler<GetAllAssurancesQuery, IEnumerable<AssuranceDto>>
{
    private readonly IAssuranceRepository _assuranceRepository;

    public GetAllAssurancesHandler(IAssuranceRepository assuranceRepository)
    {
        _assuranceRepository = assuranceRepository;
    }

    public async Task<IEnumerable<AssuranceDto>> Handle(GetAllAssurancesQuery request, CancellationToken cancellationToken)
    {
        var assurances = await _assuranceRepository.GetAllAsync();
        return assurances.ToDtoList();
    }
}



